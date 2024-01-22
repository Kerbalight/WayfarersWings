using KSP.Sim.impl;
using UnityEngine.Serialization;
using WayfarersWings.Managers;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Session;

public class KerbalWingEntry
{
    public Wing Wing;
    public IGGuid KerbalId;
    public DateTime UnlockedAt;
    public double UniverseTime;
    public bool isSuperseeded;

    public KerbalWingEntry(Wing wing, IGGuid kerbalId, DateTime unlockedAt, double universeTime, bool isSuperseeded)
    {
        Wing = wing;
        KerbalId = kerbalId;
        UnlockedAt = unlockedAt;
        this.UniverseTime = universeTime;
        this.isSuperseeded = isSuperseeded;
    }
}