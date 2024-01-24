using KSP.Messages;
using WayfarersWings.Models.Conditions.Events;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Conditions;

// TODO We could do something (with GetTriggers) to make this work with any event
[ConditionTriggerEvent(typeof(FlagPlantedMessage))]
public class FlagPlantedCondition : BaseCondition
{
    public override bool IsValid(Transaction transaction)
    {
        return transaction?.Message is FlagPlantedMessage;
    }
}