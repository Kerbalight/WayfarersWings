using KSP.Messages;
using KSP.Sim.impl;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using WayfarersWings.Models.Conditions.Events;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Conditions;

/// <summary>
/// Checks the Vessel's situation.
/// </summary>
[Serializable]
[ConditionTriggerEvent(typeof(VesselSituationChangedMessage))]
[ConditionTriggerEvent(typeof(VesselLandedGroundAtRestMessage))]
[ConditionTriggerEvent(typeof(VesselLandedWaterAtRestMessage))]
public class VesselCondition : BaseCondition
{
    [JsonConverter(typeof(StringEnumConverter))]
    public VesselSituations? situation;

    // By default, we don't want to trigger on EVAs.
    public bool? isEva = false;
    public bool? isAtRest;

    public override bool IsValid(Transaction transaction)
    {
        if (transaction?.Vessel == null)
            return false;

        // We don't want to trigger on Kerbal EVAs. Use KerbalCondition for that.
        if (isEva != null && transaction.Vessel.IsKerbalEVA != isEva)
            return false;

        if (situation != null && transaction.Vessel?.Situation != situation)
            return false;
        if (RequiresLandedOrSplashed() && (transaction.Vessel.AltitudeFromTerrain > 100))
            return false;
        if (isAtRest != null && transaction.Vessel.IsVesselAtRest() != isAtRest)
            return false;

        return true;
    }

    public bool RequiresLandedOrSplashed()
    {
        return situation is VesselSituations.Landed or VesselSituations.Splashed;
    }

    public override void Configure()
    {
    }
}