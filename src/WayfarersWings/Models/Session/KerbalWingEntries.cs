using BepInEx.Logging;
using KSP.Sim.impl;
using WayfarersWings.Managers;
using WayfarersWings.Models.Configs;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Session;

/// <summary>
/// Tracks the wings that have been unlocked by the Kerbal.
/// </summary>
public class KerbalWingEntries
{
    public static readonly ManualLogSource Logger =
        BepInEx.Logging.Logger.CreateLogSource("WayfarersWings.KerbalWingEntries");

    public IGGuid KerbalId;
    private readonly List<KerbalWingEntry> _entries = [];
    public IEnumerable<KerbalWingEntry> Entries => _entries.AsReadOnly();

    private HashSet<string> _unlockedWingsCodes = [];

    public KerbalWingEntries(IGGuid kerbalId)
    {
        KerbalId = kerbalId;
    }

    public KerbalWingEntries(KerbalWingEntriesData data)
    {
        KerbalId = data.kerbalId;
        foreach (var entryData in data.entries)
        {
            if (!Core.Instance.WingsPool.TryGetWingByCode(entryData.wingCode, out var wing))
            {
                Logger.LogWarning("Failed to find wing with code '" + entryData.wingCode + "' for " + KerbalId);
                continue;
            }

            var entry = new KerbalWingEntry(
                wing,
                kerbalId: KerbalId,
                unlockedAt: entryData.unlockedAt,
                universeTime: entryData.universeTime,
                isSuperseeded: entryData.isSuperseeded
            );
            UpdateUnlockedWingCodes(entry.Wing);
            _entries.Add(entry);
        }
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

    public void AddWing(Wing wing)
    {
        var universeTime = Core.GetUniverseTime();
        var entry = new KerbalWingEntry(
            wing,
            kerbalId: KerbalId,
            unlockedAt: DateTime.Now,
            universeTime: universeTime,
            isSuperseeded: false
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

    private void UpdateUnlockedWingCodes(Wing wing)
    {
        _unlockedWingsCodes.Add(wing.config.name);

        // If we unlock the "first" wing, unlock the "not first" wing too
        if (wing.config.isFirst)
            _unlockedWingsCodes.Add(WingsPool.GetNotFirstWingName(wing));
    }
}