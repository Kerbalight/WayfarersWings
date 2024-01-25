using JetBrains.Annotations;
using KSP.Messages;
using KSP.Sim.impl;
using WayfarersWings.Models.Conditions.Events;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Conditions;

[ConditionTriggerEvent(typeof(EVAEnteredMessage))]
[ConditionTriggerEvent(typeof(EVALeftMessage))]
public class KerbalCondition : BaseCondition
{
    /// <summary>
    /// This is always true; for now this condition works only on `KerbalComponent`
    /// </summary>
    public bool? isEva = true;

    public override bool IsValid(Transaction transaction)
    {
        // Attention: this condition is NOT valid on EVALeftMessage since
        // the kerbal is already set as `IsKerbalEVA = false` when the message is dispatched
        if (transaction.Vessel is not { IsKerbalEVA: true }) return false;

        return true;
    }
}