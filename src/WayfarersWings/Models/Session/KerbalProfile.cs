using BepInEx.Logging;
using KSP.Game;
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

    /// <summary>
    /// Missions completed by the Kerbal
    /// </summary>
    public int missionsCount = 0;

    public double? lastLaunchedAt;
    public double lastMissionTime = 0;
    public double totalMissionTime = 0;

    public double? lastEvaEnteredAt;
    public double lastEvaTime = 0;
    public double totalEvaSpaceTime = 0;
    public double totalEvaAtmosphereTime = 0;

    /// <summary>
    /// Check if this Kerbal is starred
    /// </summary>
    public bool isStarred = false;

    [JsonIgnore]
    public double TotalEvaTime => totalEvaSpaceTime + totalEvaAtmosphereTime;

    [JsonIgnore]
    public KerbalInfo KerbalInfo
    {
        get
        {
            if (!WingsSessionManager.Roster.TryGetKerbalByID(kerbalId, out var kerbalInfo))
                throw new Exception("Failed to find KerbalInfo for " + kerbalId);

            return kerbalInfo;
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
                    return false;
            }
        }

        return true;
    }

    public KerbalStatus GetStatus()
    {
        if (!WingsSessionManager.Roster.TryGetKerbalByID(kerbalId, out var kerbalInfo))
            return KerbalStatus.Unknown;

        if (kerbalInfo.Location.SimObjectId.Equals(WingsSessionManager.Roster.KSCGuid))
            return KerbalStatus.Available;

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
                if (awarded.Wing.config.chain == wing.config.chain &&
                    awarded.Wing.config.points < wing.config.points)
                {
                    awarded.isSuperseeded = true;
                }
            }
        }


        UpdateUnlockedWingCodes(wing);
        _entries.Add(entry);
    }

    public void RevokeWing(KerbalWingEntry entry)
    {
        var wing = entry.Wing;
        _entries.Remove(entry);

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
                maxPointsEntry.isSuperseeded = false;
            }
        }
    }

    private void UpdateUnlockedWingCodes(Wing wing)
    {
        _unlockedWingsCodes.Add(wing.config.name);

        // If we unlock the "first" wing, unlock the "not first" wing too
        if (wing.config.isFirst)
            _unlockedWingsCodes.Add(WingsPool.GetNotFirstWingName(wing));
    }

    public void CompleteMission(VesselComponent vessel)
    {
        missionsCount++;

        if (!lastLaunchedAt.HasValue)
        {
            Logger.LogWarning("lastLaunchedAt is null, cannot CompleteMission. Discarding.");
            return;
        }

        lastMissionTime = Core.GetUniverseTime() - lastLaunchedAt.Value;
        totalMissionTime += lastMissionTime;

        Logger.LogDebug("Added " + lastMissionTime + "s mission time to " + KerbalInfo.Attributes.GetFullName());
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
            totalEvaAtmosphereTime += evaTime;
        else
            totalEvaSpaceTime += evaTime;

        lastEvaTime = evaTime;
        lastEvaEnteredAt = null;

        Logger.LogDebug("Added " + evaTime + "s EVA time to " + KerbalInfo.Attributes.GetFullName());
    }

    /// <summary>
    /// We can't just use OnDeserialize since we need to access data available only
    /// after game load has finished (e.g. the WingsPool).
    /// </summary>
    public void OnAfterGameLoad()
    {
        foreach (var entry in _entries)
        {
            if (!Core.Instance.WingsPool.TryGetWingByCode(entry.wingCode, out entry.Wing))
            {
                Logger.LogWarning($"[kerbal={kerbalId}] Failed to find wing with code '{entry.wingCode}'");
                _errored.Add(entry);
                continue;
            }

            entry.KerbalId = kerbalId;
            entry.OnAfterGameLoad();
            UpdateUnlockedWingCodes(entry.Wing);
        }

        foreach (var entry in _errored)
        {
            _entries.Remove(entry);
        }
    }

    public void OnBeforeGameSave()
    {
        foreach (var entry in _entries)
        {
            entry.OnBeforeGameSave();
        }
    }
}