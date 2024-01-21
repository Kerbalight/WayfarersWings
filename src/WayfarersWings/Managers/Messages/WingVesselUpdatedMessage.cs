using KSP.Sim.impl;

namespace WayfarersWings.Managers.Messages;

public abstract class WingVesselUpdatedMessage : WingBaseMessage
{
    public VesselComponent Vessel { get; }

    protected WingVesselUpdatedMessage(VesselComponent vessel)
    {
        Vessel = vessel;
    }
}