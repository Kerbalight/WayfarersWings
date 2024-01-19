using KSP.Messages;
using WayfarersWings.Models.Configs;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Conditions;

/// <summary>
/// Check the vessel's orbit parameters
/// </summary>
public class OrbitCondition : BaseCondition
{
    /// Check if the vessel is in a stable orbit
    public bool? IsStable;

    /// Check if the vessel's eccentricity is less than this value
    public float? MaxEccentricity;

    /// Check if the vessel's eccentricity is greater than this value
    public float? MinEccentricity;

    public override bool IsValid(Transaction transaction)
    {
        // if (transaction.Message is not StableOrbitCreatedMessage message) ;
        if (IsStable == true && !(transaction.Vessel?.Orbiter?.isStable ?? false))
            return false;
        if (MaxEccentricity != null && transaction.Vessel?.Orbit.eccentricity > MaxEccentricity)
            return false;
        if (MinEccentricity != null && transaction.Vessel?.Orbit.eccentricity < MinEccentricity)
            return false;

        return true;
    }

    public override void Configure(ConditionConfig config)
    {
        if (config.TryGetDatum("isStable", out bool isStable))
            IsStable = isStable;
        if (config.TryGetDatum("maxEccentricity", out float maxEccentricity))
            MaxEccentricity = maxEccentricity;
        if (config.TryGetDatum("minEccentricity", out float minEccentricity))
            MinEccentricity = minEccentricity;
    }
}