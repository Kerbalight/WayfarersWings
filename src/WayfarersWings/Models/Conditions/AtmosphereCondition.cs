using WayfarersWings.Managers.Observer;
using WayfarersWings.Managers.Observer.Properties;
using WayfarersWings.Managers.Observer.Properties.Events;
using WayfarersWings.Managers.Observer.Properties.Types;
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

    public override void Configure()
    {
        if (minAtmDensity.HasValue)
            VesselsStateObserver.Instance.ObservePropertyAtPoint<VesselObservedAtmDensity>(minAtmDensity.Value);
        if (maxAtmDensity.HasValue)
            VesselsStateObserver.Instance.ObservePropertyAtPoint<VesselObservedAtmDensity>(maxAtmDensity.Value);
    }
}