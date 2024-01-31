using KSP.Messages;
using WayfarersWings.Models.Conditions.Events;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Conditions;

[ConditionTriggerEvent(typeof(VesselDockedMessage))]
public class DockingCondition : BaseCondition
{
    public bool? isRightAfterDocking;

    public override bool IsValid(Transaction transaction)
    {
        if (isRightAfterDocking.HasValue && transaction.Message is not VesselDockedMessage)
            return false;

        return true;
    }
}