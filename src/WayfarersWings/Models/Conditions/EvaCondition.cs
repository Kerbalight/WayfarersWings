using JetBrains.Annotations;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Conditions;

// TODO Rename in KerbalCondition
[UsedImplicitly]
public class EvaCondition : BaseCondition
{
    public override bool IsValid(Transaction transaction)
    {
        return transaction.Vessel?.IsKerbalEVA ?? false;
    }
}