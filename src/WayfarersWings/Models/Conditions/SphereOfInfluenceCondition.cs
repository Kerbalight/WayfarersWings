using JetBrains.Annotations;
using KSP.Messages;
using KSP.Sim.impl;
using WayfarersWings.Models.Conditions.Events;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Conditions;

[Serializable]
[ConditionTriggerEvent(typeof(SOIEnteredMessage))]
public class SphereOfInfluenceCondition : CelestialBodyCondition
{
    /// <summary>
    /// Whether the condition should be triggered right after the SOI change
    /// or it's enough to be in the SOI
    /// </summary>
    public bool? isRightAfterSOIChange = null;

    public override bool IsValid(Transaction transaction)
    {
        if (isRightAfterSOIChange.HasValue && isRightAfterSOIChange.Value)
        {
            if (transaction.Message is not SOIEnteredMessage soiEnteredMessage) return false;

            // TODO Double check
            if (soiEnteredMessage.bodyEntered.isHomeWorld &&
                soiEnteredMessage.bodyExited is null or { isHomeWorld: true })
            {
                return false;
            }

            if (!soiEnteredMessage.bodyEntered.Equals(CelestialBody)) return false;

            return true;
        }

        return transaction.Vessel?.mainBody?.Equals(CelestialBody) ?? false;
    }
}