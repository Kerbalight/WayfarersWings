using KSP.Sim.impl;
using UnityEngine.Serialization;

namespace WayfarersWings.Models.Session;

[Serializable]
public class KerbalWingEntriesData
{
    public IGGuid kerbalId;
    public List<KerbalWingEntryData> entries = [];

    public KerbalWingEntriesData()
    {
    }

    public KerbalWingEntriesData(KerbalWingEntries kerbalWingEntries)
    {
        kerbalId = kerbalWingEntries.KerbalId;

        foreach (var entry in kerbalWingEntries.Entries)
        {
            entries.Add(new KerbalWingEntryData()
            {
                wingCode = entry.Wing.config.name,
                unlockedAt = entry.UnlockedAt,
                universeTime = entry.UniverseTime,
                isSuperseeded = entry.isSuperseeded
            });
        }
    }
}