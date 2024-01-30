using WayfarersWings.Managers.Observer.Properties.Events;
using WayfarersWings.Models.Conditions.Events;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Conditions;

[ConditionTriggerEvent(typeof(WingVesselAtmDensityUpdatedMessage))]
public class AtmosphereCondition : BaseCondition
{
    public int? minAtmDensity { get; set; }
    public int? maxAtmDensity { get; set; }

    public override bool IsValid(Transaction transaction)
    {
        if (minAtmDensity.HasValue && !(transaction.ObservedState?.atmDensity.Value > minAtmDensity.Value))
            return false;
        if (maxAtmDensity.HasValue && !(transaction.ObservedState?.atmDensity.Value < maxAtmDensity.Value))
            return false;

        return true;
    }
}