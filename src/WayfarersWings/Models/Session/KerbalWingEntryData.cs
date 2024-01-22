using KSP.Sim.impl;

namespace WayfarersWings.Models.Session;

[Serializable]
public class KerbalWingEntryData
{
    public string wingCode;
    public DateTime unlockedAt;
    public double universeTime;
    public bool isSuperseeded;
}