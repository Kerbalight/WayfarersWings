using JetBrains.Annotations;
using KSP.Messages;
using KSP.Sim.impl;
using WayfarersWings.Models.Conditions.Events;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Conditions;

[ConditionTriggerEvent(typeof(EVAEnteredMessage))]
[ConditionTriggerEvent(typeof(EVALeftMessage))]
public class KerbalEVACondition : BaseCondition
{
    /// <summary>
    /// This is always true; this condition is meant to check for Kerbal EVA.
    /// For profile things, like checking missions, use the `KerbalProfileCondition`
    /// </summary>
    public bool? isEva = true;

    /// <summary>
    /// Condition is valid only if the kerbal is in space/in atmosphere
    /// </summary>
    public bool? isInAtmosphere;

    public override bool IsValid(Transaction transaction)
    {
        // I'd like to use `isInAtmosphere` here, but the TelemetryComponent is not
        // instantiated yet when the EVAEnteredMessage is dispatched, so
        // all the data (like inAtmosphere) is not available.
        if (transaction.Message is EVAEnteredMessage) return false;

        // Attention: this condition is NOT valid on EVALeftMessage since
        // the kerbal is already set as `IsKerbalEVA = false` when the message is dispatched
        // if (transaction.Vessel is not { IsKerbalEVA: true }) return false;

        // This condition instead is valid on both messages to detect EVAs
        if (transaction.SimulationObject?.Kerbal?.KerbalInfo == null) return false;

        if (isInAtmosphere.HasValue && transaction.Vessel?.IsInAtmosphere != isInAtmosphere)
            return false;

        return true;
    }
}