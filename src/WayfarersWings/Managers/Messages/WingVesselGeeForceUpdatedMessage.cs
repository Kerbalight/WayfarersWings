using KSP.Sim.impl;

namespace WayfarersWings.Managers.Messages;

public class WingVesselGeeForceUpdatedMessage : WingVesselUpdatedMessage
{
    public int GeeForce { get; }

    public WingVesselGeeForceUpdatedMessage(VesselComponent vessel, int geeForce) : base(vessel)
    {
        GeeForce = geeForce;
    }
}