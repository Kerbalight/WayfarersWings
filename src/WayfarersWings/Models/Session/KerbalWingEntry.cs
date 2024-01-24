using KSP.Sim.impl;
using Newtonsoft.Json;
using UnityEngine.Serialization;
using WayfarersWings.Managers;
using WayfarersWings.Models.Session.Json;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Session;

[Serializable]
public class KerbalWingEntry : IJsonSaved
{
    [JsonIgnore]
    public Wing Wing;

    public string wingCode;
    public DateTime unlockedAt;
    public double universeTime;
    public bool isSuperseeded;

    public KerbalWingEntry() { }

    public KerbalWingEntry(Wing wing, DateTime unlockedAt, double universeTime, bool isSuperseeded)
    {
        Wing = wing;
        wingCode = wing.config.name;
        this.unlockedAt = unlockedAt;
        this.universeTime = universeTime;
        this.isSuperseeded = isSuperseeded;
    }

    public void OnAfterGameLoad() { }

    public void OnBeforeGameSave()
    {
        wingCode = Wing.config.name;
    }
}