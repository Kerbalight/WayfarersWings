using KSP.Game;
using KSP.Sim.impl;
using WayfarersWings.Managers.Messages;

namespace WayfarersWings.Managers.Observer.Properties;

public class VesselObservedGeeForce : VesselObservedProperty<int>
{
    public override int GetValue(VesselComponent vessel)
    {
        return (int)vessel.geeForce;
    }

    protected override WingVesselUpdatedMessage CreateTriggeredMessage(VesselComponent vessel)
    {
        return new WingVesselGeeForceUpdatedMessage(vessel, Value);
    }
}