using KSP.Messages;
using WayfarersWings.Managers.Messages;
using WayfarersWings.Models.Conditions.Events;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Conditions;

[Serializable]
[ConditionTriggerEvent(typeof(WingVesselGeeForceUpdatedMessage))]
public class TravelCondition : BaseCondition
{
    public int? maxGeeForce;
    public int? minGeeForce;

    public override bool IsValid(Transaction transaction)
    {
        if (transaction.ObservedState == null)
            return false;

        if (maxGeeForce.HasValue && !(transaction.ObservedState.geeForce.Value <= maxGeeForce))
            return false;
        if (minGeeForce.HasValue && !(transaction.ObservedState.geeForce.Value >= minGeeForce))
            return false;

        return true;
    }
}