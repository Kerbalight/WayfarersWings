using KSP.Sim.impl;
using UnityEngine.Serialization;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Session;

public class KerbalWingEntry
{
    public Wing Wing;
    public IGGuid KerbalId;
    public DateTime UnlockedAt;
    public double UniverseTime;

    public KerbalWingEntry(Wing wing, IGGuid kerbalId, DateTime unlockedAt, double universeTime)
    {
        Wing = wing;
        KerbalId = kerbalId;
        UnlockedAt = unlockedAt;
        this.UniverseTime = universeTime;
    }
}