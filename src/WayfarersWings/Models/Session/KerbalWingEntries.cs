using KSP.Sim.impl;
using WayfarersWings.Managers;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Session;

/// <summary>
/// Tracks the wings that have been unlocked by the Kerbal.
/// </summary>
public class KerbalWingEntries
{
    public IGGuid KerbalId;
    private readonly List<KerbalWingEntry> _entries = [];
    public IEnumerable<KerbalWingEntry> Entries => _entries.AsReadOnly();

    private HashSet<string> _unlockedWingsCodes = [];

    public KerbalWingEntries(IGGuid kerbalId)
    {
        KerbalId = kerbalId;
    }

    public bool HasWing(Wing wing)
    {
        return _unlockedWingsCodes.Contains(wing.config.name);
    }

    public void AddWing(Wing wing)
    {
        var universeTime = Core.GetUniverseTime();
        var entry = new KerbalWingEntry(
            wing,
            kerbalId: KerbalId,
            unlockedAt: DateTime.Now,
            universeTime: universeTime
        );
        _unlockedWingsCodes.Add(wing.config.name);

        // If we unlock the "first" wing, unlock the "not first" wing too
        if (wing.config.isFirst)
            _unlockedWingsCodes.Add(WingsPool.GetNotFirstWingName(wing));

        _entries.Add(entry);
    }
}