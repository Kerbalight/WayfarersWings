using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Conditions;

public class EvaCondition : BaseCondition
{
    public override bool IsValid(Transaction transaction)
    {
        return transaction.Vessel?.IsKerbalEVA ?? false;
    }
}