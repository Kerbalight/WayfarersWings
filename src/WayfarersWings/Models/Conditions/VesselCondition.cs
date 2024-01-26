using KSP.Messages;
using KSP.Modules;
using KSP.Sim.DeltaV;
using KSP.Sim.impl;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using WayfarersWings.Managers;
using WayfarersWings.Managers.Analyzers;
using WayfarersWings.Models.Conditions.Events;
using WayfarersWings.Models.Wings;
using WayfarersWings.Utility.Serialization;

namespace WayfarersWings.Models.Conditions;

/// <summary>
/// Checks the Vessel's situation.
/// </summary>
[Serializable]
[ConditionTriggerEvent(typeof(VesselSituationChangedMessage))]
[ConditionTriggerEvent(typeof(VesselLaunchedMessage))]
[ConditionTriggerEvent(typeof(VesselLandedGroundAtRestMessage))]
[ConditionTriggerEvent(typeof(VesselLandedWaterAtRestMessage))]
public class VesselCondition : BaseCondition
{
    [JsonConverter(typeof(StringEnumConverter))]
    public VesselSituations? situation;

    /// <summary>
    /// When set, the condition will only trigger if the vessel's situation
    /// is changed from this situation; this requires the VesselSituationChangedMessage
    /// to be triggered.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public VesselSituations? previousSituation;

    /// <summary>
    /// When set, the condition will only trigger if the vessel's previous
    /// situation is _not_ this situation
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public VesselSituations? skipPreviousSituation;

    // By default, we don't want to trigger on EVAs.
    public bool? isEva = false;
    public bool? isAtRest;
    public bool? isInAtmosphere;
    public bool? isRightAfterLaunch;
    public bool? isRightAfterLandingGroundAtRest;
    public bool? isRightAfterLandingWaterAtRest;
    public bool? isRightAfterLandingAtRest;

    [JsonConverter(typeof(GameTimeSpanJsonConverter))]
    public GameTimeSpan? maxTimeFromLaunch;

    [JsonConverter(typeof(GameTimeSpanJsonConverter))]
    public GameTimeSpan? minTimeFromLaunch;

    /// <summary>
    /// Checks the percentage of solid fuel in the vessel
    /// </summary>
    public double? minSolidFuelMassPercentage;

    /// <summary>
    /// Checks the total mass of the vessel.
    /// Not sure about how we should handle docking / how it is handled by KSP2
    /// </summary>
    public double? minMass;

    public double? maxMass;

    public double? minAltitudeSeaLevel;
    public double? maxAltitudeSeaLevel;
    public double? minAltitudeTerrain;

    // public double? didUseParachutes;

    public override bool IsValid(Transaction transaction)
    {
        if (transaction?.Vessel == null)
            return false;

        // We don't want to trigger on Kerbal EVAs. Use KerbalCondition for that.
        if (isEva != null && transaction.Vessel.IsKerbalEVA != isEva)
            return false;

        // TODO Maybe use VesselObservedState?
        if (previousSituation.HasValue)
        {
            if (transaction.Message is not VesselSituationChangedMessage situationChangedMessage)
                return false;

            if (situationChangedMessage.OldSituation != previousSituation) return false;
            if (situation.HasValue && situationChangedMessage.NewSituation != situation)
                return false;
        }

        if (isRightAfterLaunch.HasValue && transaction.Message is not VesselLaunchedMessage)
            return false;
        if (isRightAfterLandingAtRest.HasValue && transaction.Message is not VesselLandedGroundAtRestMessage &&
            transaction.Message is not VesselLandedWaterAtRestMessage)
            return false;
        if (isRightAfterLandingGroundAtRest.HasValue && transaction.Message is not VesselLandedGroundAtRestMessage)
            return false;
        if (isRightAfterLandingWaterAtRest.HasValue && transaction.Message is not VesselLandedWaterAtRestMessage)
            return false;

        // Test for VesselObservedState
        if (skipPreviousSituation.HasValue && transaction.ObservedState?.previousSituation == skipPreviousSituation)
            return false;

        if (isInAtmosphere.HasValue && transaction.Vessel?.IsInAtmosphere != isInAtmosphere)
            return false;

        if (situation != null && transaction.Vessel?.Situation != situation)
            return false;
        if (RequiresLandedOrSplashed() && (transaction.Vessel.AltitudeFromTerrain > 100))
            return false;
        if (isAtRest.HasValue && transaction.Vessel.IsVesselAtRest() != isAtRest)
            return false;
        if (maxTimeFromLaunch.HasValue &&
            !(Core.GetUniverseTime() - transaction.Vessel.launchTime < maxTimeFromLaunch.Value.Seconds))
            return false;
        if (minTimeFromLaunch.HasValue &&
            !(Core.GetUniverseTime() - transaction.Vessel.launchTime > minTimeFromLaunch.Value.Seconds))
            return false;

        if (minAltitudeSeaLevel.HasValue && !(transaction.Vessel.AltitudeFromSeaLevel > minAltitudeSeaLevel.Value))
            return false;
        if (maxAltitudeSeaLevel.HasValue && !(transaction.Vessel.AltitudeFromSeaLevel < maxAltitudeSeaLevel.Value))
            return false;
        if (minAltitudeTerrain.HasValue && !(transaction.Vessel.AltitudeFromTerrain > minAltitudeTerrain.Value))
            return false;

        // Mass
        if (minMass.HasValue && !(transaction.Vessel.totalMass > minMass.Value))
            return false;
        if (maxMass.HasValue && !(transaction.Vessel.totalMass < maxMass.Value))
            return false;

        // Expensive checks, we check them last
        if (minSolidFuelMassPercentage.HasValue && !(VesselPartsAnalyzer.GetSolidFuelPercentage(transaction.Vessel) >
                                                     minSolidFuelMassPercentage))
            return false;

        return true;
    }

    public bool RequiresLandedOrSplashed()
    {
        return situation is VesselSituations.Landed or VesselSituations.Splashed;
    }

    public override void Configure() { }
}