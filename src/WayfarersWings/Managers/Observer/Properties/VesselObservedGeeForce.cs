using KSP.Game;
using KSP.Sim.impl;
using WayfarersWings.Managers.Messages;
using WayfarersWings.Managers.Observer.Properties.Events;
using WayfarersWings.Managers.Observer.Properties.Types;

namespace WayfarersWings.Managers.Observer.Properties;

public class VesselObservedGeeForce : VesselObservedSustainedProperty
{
    // Ignore the first 2G
    public override bool HasChanged => Value != PreviousValue && Value >= 3;

    protected override int GetInstantValue(VesselComponent vessel)
    {
        return (int)vessel.geeForce;
    }

    protected override WingVesselUpdatedMessage CreateTriggeredMessage(VesselComponent vessel)
    {
        return new WingVesselGeeForceUpdatedMessage(vessel, Value);
    }
}