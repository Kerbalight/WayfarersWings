using KSP.Messages;
using WayfarersWings.Managers.Messages;
using WayfarersWings.Managers.Observer.Properties.Events;
using WayfarersWings.Models.Conditions.Events;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Conditions;

[Serializable]
[ConditionTriggerEvent(typeof(WingVesselGeeForceUpdatedMessage))]
[ConditionTriggerEvent(typeof(WingVesselHasMovedOnSurfaceUpdatedMessage))]
public class TravelCondition : BaseCondition
{
    public int? maxGeeForce;
    public int? minGeeForce;

    /// <summary>
    /// Check if the rover vessel has moved on the surface.
    /// </summary>
    public bool? hasRoverMovedOnSurface;

    public override bool IsValid(Transaction transaction)
    {
        if (transaction.ObservedState == null)
            return false;

        if (maxGeeForce.HasValue && !(transaction.ObservedState.geeForce.Value <= maxGeeForce))
            return false;
        if (minGeeForce.HasValue && !(transaction.ObservedState.geeForce.Value >= minGeeForce))
            return false;
        if (hasRoverMovedOnSurface.HasValue &&
            transaction.ObservedState.hasMovedOnSurface.Value != hasRoverMovedOnSurface)
            return false;

        return true;
    }
}