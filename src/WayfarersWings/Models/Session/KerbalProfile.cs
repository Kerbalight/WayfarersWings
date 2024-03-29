﻿using BepInEx.Logging;
using KSP.Game;
using KSP.Game.Science;
using KSP.Sim.impl;
using KSP.Sim.ResourceSystem;
using Newtonsoft.Json;
using UnityEngine.Serialization;
using WayfarersWings.Managers;
using WayfarersWings.Models.Configs;
using WayfarersWings.Models.Session.Json;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Session;

public enum KerbalStatus
{
    Unknown,
    Available,
    Assigned,
    AssignedActive,
    Dead
}

/// <summary>
/// Tracks the wings that have been unlocked by the Kerbal and other
/// attributes handled by Wayfarer's Wings
/// </summary>
[Serializable]
public class KerbalProfile : IJsonSaved
{
    public static readonly ManualLogSource Logger =
        BepInEx.Logging.Logger.CreateLogSource("WayfarersWings.KerbalWingEntries");

    /// <summary>
    /// Reference to the Kerbal ID
    /// </summary>
    public IGGuid kerbalId;

    [JsonProperty("entries")]
    private List<KerbalWingEntry> _entries = [];

    [JsonProperty("errored")]
    private List<KerbalWingEntry> _errored = [];

    // We keep a list of unlocked wings codes to avoid iterating over _entries
    [JsonIgnore]
    private readonly HashSet<string> _unlockedWingsCodes = [];

    /// <summary>
    /// Wings unlocked by the Kerbal. We store additional metadata for each
    /// Entry.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<KerbalWingEntry> Entries => _entries.AsReadOnly();

    [JsonIgnore]
    public int totalPoints = 0;

    /// <summary>
    /// Missions completed by the Kerbal
    /// </summary>
    public int missionsCount = 0;

    public double? lastLaunchedAt;
    public double lastMissionTime = 0;
    public double totalMissionTime = 0;

    /// <summary>
    /// Used as a reference to display mission reports. It's the value of
    /// "lastLaunchedAt" when the vessel is recovered.
    /// </summary>
    public double? lastMissionLaunchedAt;

    public double? lastMissionStartedAt;

    public double? lastEvaEnteredAt;
    public double lastEvaTime = 0;
    public double lastEvaSpaceTime = 0;
    public double lastEvaAtmosphereTime = 0;
    public double totalEvaSpaceTime = 0;
    public double totalEvaAtmosphereTime = 0;

    // Tracks visited bodies and regions


    public HashSet<string> missionBodies = [];

    /// <summary>
    /// Regions the Kerbal has visited in current mission
    /// (Both discoverable and biomes)
    /// </summary>
    public HashSet<VisitedRegion> missionRegions = [];

    /// <summary>
    /// All the bodies the Kerbal has visited in its career
    /// </summary>
    public HashSet<string> visitedBodies = [];

    /// <summary>
    /// All the Biomes the Kerbal has visited in its career
    /// </summary
    public HashSet<VisitedRegion> visitedBiomes = [];

    /// <summary>
    /// All the Discoverables the Kerbal has visited in its career
    /// </summary>
    public HashSet<VisitedRegion> visitedDiscoverables = [];

    [JsonIgnore]
    public int VisitedDiscoverablesWithoutKSC => Math.Max(0, visitedDiscoverables.Count - 1);

    /// <summary>
    /// Check if this Kerbal is starred
    /// </summary>
    public bool isStarred = false;

    /// <summary>
    /// We store the full name here to avoid having to access the KerbalInfo
    /// </summary>
    public string? fullName;

    [JsonIgnore]
    public double TotalEvaTime => totalEvaSpaceTime + totalEvaAtmosphereTime;

    [JsonIgnore]
    public bool IsInEVA => lastEvaEnteredAt.HasValue;

    [JsonIgnore]
    public bool HasJustLaunched => lastLaunchedAt.HasValue && Core.GetUniverseTime() - lastLaunchedAt < 1;

    [JsonIgnore]
    public KerbalInfo? KerbalInfo
    {
        get
        {
            if (WingsSessionManager.Roster.TryGetKerbalByID(kerbalId, out var kerbalInfo))
                return kerbalInfo;

            Logger.LogInfo($"Failed to find KerbalInfo for {fullName} (id={kerbalId}), setting as dead");
            return null;
        }
    }

    public KerbalProfile() { }

    public KerbalProfile(IGGuid kerbalId)
    {
        this.kerbalId = kerbalId;
    }

    public bool HasWing(Wing wing)
    {
        return _unlockedWingsCodes.Contains(wing.config.name);
    }

    public bool IsAwardable(Wing wing)
    {
        if (HasWing(wing)) return false;
        if (wing.config.chain != null)
        {
            foreach (var awarded in _entries)
            {
                if (awarded.Wing.config.chain == wing.config.chain &&
                    awarded.Wing.config.points >= wing.config.points)
                {
                    Logger.LogDebug("Wing " + wing.config.name + " is superseded by " + awarded.Wing.config.name +
                                    ", will not award");
                    return false;
                }
            }
        }

        return true;
    }

    public KerbalStatus GetStatus()
    {
        if (!WingsSessionManager.Roster.TryGetKerbalByID(kerbalId, out var kerbalInfo))
            return KerbalStatus.Dead;

        if (kerbalInfo.Location.SimObjectId.Equals(WingsSessionManager.Roster.KSCGuid))
            return KerbalStatus.Available;

        if (Core.ActiveVessel != null && WingsSessionManager.Roster.GetAllKerbalsInVessel(Core.ActiveVessel.GlobalId)
                .Any(k => k.Id == kerbalId))
            return KerbalStatus.AssignedActive;

        return KerbalStatus.Assigned;
    }


    public void AddWing(Wing wing)
    {
        var universeTime = Core.GetUniverseTime();
        var entry = new KerbalWingEntry(
            wing,
            unlockedAt: DateTime.Now,
            universeTime: universeTime,
            isSuperseeded: false,
            kerbalId: kerbalId
        );

        if (wing.config.chain != null)
        {
            foreach (var awarded in _entries)
            {
                if (awarded.Wing.config.chain != wing.config.chain ||
                    awarded.Wing.config.points >= wing.config.points ||
                    awarded.isSuperseeded) continue;

                awarded.isSuperseeded = true;
                totalPoints -= awarded.Wing.config.points;
                Logger.LogDebug(
                    $"Wing {awarded.Wing.config.name} is superseded by {wing.config.name}, will be replaced");
            }
        }


        UpdateUnlockedWingCodes(wing);
        _entries.Add(entry);
        totalPoints += wing.config.points;
        Logger.LogDebug($"Added wing {wing.config.name} to {KerbalInfo?.Attributes.GetFullName()}");

        SortEntriesByPoints();
    }

    public void RevokeWing(KerbalWingEntry entry)
    {
        var wing = entry.Wing;
        _entries.Remove(entry);
        totalPoints -= wing.config.points;

        _unlockedWingsCodes.Remove(wing.config.name);
        if (wing.config.isFirst)
            _unlockedWingsCodes.Remove(WingsPool.GetNotFirstWingName(wing));


        if (wing.config.chain != null)
        {
            var maxPoints = 0;
            var maxPointsEntry = (KerbalWingEntry?)null;
            foreach (var awarded in _entries)
            {
                if (awarded.Wing.config.chain == wing.config.chain &&
                    awarded.Wing.config.points > maxPoints)
                {
                    maxPoints = awarded.Wing.config.points;
                    maxPointsEntry = awarded;
                }
            }

            // Restore the superseded status of the highest one,
            // now that we removed the one that superseded it
            if (maxPointsEntry != null)
            {
                Logger.LogDebug($"Wing {maxPointsEntry.Wing.config.name} is no longer superseded");
                maxPointsEntry.isSuperseeded = false;
                totalPoints += maxPointsEntry.Wing.config.points;
            }
        }

        SortEntriesByPoints();
        Logger.LogDebug($"Revoked wing {wing.config.name} to {KerbalInfo?.Attributes.GetFullName()}");
    }

    /// <summary>
    /// We do it here to avoid having to sort the list every time we
    /// need to render the UI.
    /// The entries are sorted by points, descending.
    /// </summary>
    private void SortEntriesByPoints()
    {
        _entries.Sort((a, b) => b.Wing.config.points.CompareTo(a.Wing.config.points));
    }

    private void UpdateUnlockedWingCodes(Wing wing)
    {
        _unlockedWingsCodes.Add(wing.config.name);

        // If we unlock the "first" wing, unlock the "not first" wing too
        if (wing.config.isFirst)
            _unlockedWingsCodes.Add(WingsPool.GetNotFirstWingName(wing));
    }

    public List<KerbalWingEntry> GetLastMissionEntries()
    {
        List<KerbalWingEntry> missionEntries = [];
        if (!lastMissionStartedAt.HasValue) return missionEntries;

        foreach (var entry in _entries)
        {
            if (entry.universeTime < lastMissionStartedAt) continue;
            if (entry.isSuperseeded) continue;
            missionEntries.Add(entry);
        }

        return missionEntries;
    }

    #region Lifetime

    /// <summary>
    /// Called when vessel is placed on the launchpad.
    /// </summary>
    public void StartMission(VesselComponent vessel)
    {
        lastMissionStartedAt = Core.GetUniverseTime();

        // Cleanup
        lastEvaEnteredAt = null;
        lastLaunchedAt = null;
        missionBodies.Clear();
        missionRegions.Clear();
        Logger.LogDebug($"Started mission for {KerbalInfo?.Attributes.GetFullName()}");
    }

    public void UpdateOnVesselLaunched(VesselComponent vessel)
    {
        // If we already launched, don't update the time. This happens when
        // the vessel undocks from another vessel.
        if (lastLaunchedAt.HasValue)
            return;

        lastLaunchedAt = Core.GetUniverseTime();
        lastMissionLaunchedAt = lastLaunchedAt;
        Logger.LogDebug($"Launched mission for {KerbalInfo?.Attributes.GetFullName()}");
    }

    /// <summary>
    /// Updates mission times after the vessel has been recovered.
    /// </summary>
    public void CompleteMission(VesselComponent vessel)
    {
        if (!lastLaunchedAt.HasValue)
        {
            Logger.LogWarning("lastLaunchedAt is null, cannot CompleteMission. Discarding.");
            return;
        }

        missionsCount++;

        lastMissionTime = Core.GetUniverseTime() - lastLaunchedAt.Value;
        totalMissionTime += lastMissionTime;

        lastLaunchedAt = null;
        missionBodies.Clear();
        missionRegions.Clear();

        Logger.LogDebug($"Added {lastMissionTime}s mission time to {KerbalInfo?.Attributes.GetFullName()}");
    }

    public void StartEVA(VesselComponent kerbalVessel)
    {
        lastEvaEnteredAt = Core.GetUniverseTime();
        Logger.LogDebug($"Started EVA for {KerbalInfo?.Attributes.GetFullName()}");
    }


    /// <summary>
    /// Updates EVA times
    /// </summary>
    /// <param name="kerbalVessel"></param>
    public void CompleteEVA(VesselComponent kerbalVessel)
    {
        if (!lastEvaEnteredAt.HasValue)
        {
            Logger.LogWarning("lastEvaEnteredAt is null, cannot CompleteEVA. Discarding.");
            return;
        }

        var evaTime = Core.GetUniverseTime() - lastEvaEnteredAt.Value;

        if (kerbalVessel.IsInAtmosphere)
        {
            lastEvaAtmosphereTime = evaTime;
            totalEvaAtmosphereTime += evaTime;
        }
        else
        {
            lastEvaSpaceTime = evaTime;
            totalEvaSpaceTime += evaTime;
        }

        lastEvaTime = evaTime;
        lastEvaEnteredAt = null;

        Logger.LogDebug($"Added {evaTime}s EVA time to {KerbalInfo?.Attributes.GetFullName()}");
    }

    /// <summary>
    /// We track visited bodies and regions to award wings based on these
    /// criteria.
    /// </summary>
    /// <param name="regionSituation">The situation the Kerbal entered</param>
    /// <param name="hasChanged">Set to true if this a situation not already tracked</param>
    public void OnScienceSituationChange(ScienceLocationRegionSituation regionSituation, out bool hasChanged)
    {
        hasChanged = false;

        var bodyName = regionSituation.ResearchLocation.BodyName;
        hasChanged |= visitedBodies.Add(bodyName);
        hasChanged |= missionBodies.Add(bodyName);

        if (string.IsNullOrEmpty(regionSituation.ResearchLocation.ScienceRegion))
            return;

        var scienceDataProvider = GameManager.Instance.Game.ScienceManager.ScienceRegionsDataProvider;
        var visitedRegion = VisitedRegion.FromLocation(regionSituation.ResearchLocation);

        if (scienceDataProvider.IsRegionADiscoverable(visitedRegion.BodyName, visitedRegion.ScienceRegion))
            hasChanged |= visitedDiscoverables.Add(visitedRegion);
        else
            hasChanged |= visitedBiomes.Add(visitedRegion);

        hasChanged |= missionRegions.Add(visitedRegion);
    }

    #endregion

    #region Loading & Saving

    /// <summary>
    /// We can't just use OnDeserialize since we need to access data available only
    /// after game load has finished (e.g. the WingsPool).
    /// </summary>
    public void OnAfterGameLoad()
    {
        var kerbalInfo = KerbalInfo;
        if (kerbalInfo != null)
            fullName ??= kerbalInfo.Attributes.GetFullName();
        foreach (var entry in _entries)
        {
            if (!Core.Instance.WingsPool.TryGetWingByCode(entry.wingCode, out entry.Wing))
            {
                Logger.LogWarning(
                    $"[kerbal={fullName}] Failed to find wing with code '{entry.wingCode}'");
                _errored.Add(entry);
                continue;
            }

            entry.KerbalId = kerbalId;
            entry.OnAfterGameLoad();
            UpdateUnlockedWingCodes(entry.Wing);
            totalPoints += entry.Wing.config.points;
        }

        foreach (var entry in _errored)
        {
            _entries.Remove(entry);
        }

        SortEntriesByPoints();

        Logger.LogInfo(
            $"[kerbal={fullName}] Loaded {Entries.Count()} wings for a total of {totalPoints} points");
    }

    public void OnBeforeGameSave()
    {
        fullName ??= KerbalInfo?.Attributes.GetFullName();

        foreach (var entry in _entries)
        {
            entry.OnBeforeGameSave();
        }
    }

    #endregion
}