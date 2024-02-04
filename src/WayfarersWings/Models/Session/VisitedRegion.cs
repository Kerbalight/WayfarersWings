using KSP.Game.Science;
using Newtonsoft.Json;

namespace WayfarersWings.Models.Session;

public class VisitedRegion : IEquatable<VisitedRegion>
{
    [JsonProperty("bodyName")]
    public string BodyName { get; }

    [JsonProperty("scienceRegion")]
    public string ScienceRegion { get; }

    public VisitedRegion(string bodyName, string scienceRegion)
    {
        BodyName = bodyName;
        ScienceRegion = scienceRegion;
    }

    public static VisitedRegion FromLocation(ResearchLocation location)
    {
        return new VisitedRegion(location.BodyName, location.ScienceRegion);
    }

    public bool Equals(VisitedRegion other)
    {
        return BodyName == other.BodyName && ScienceRegion == other.ScienceRegion;
    }

    public override bool Equals(object? obj)
    {
        return obj is VisitedRegion other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BodyName, ScienceRegion);
    }
}