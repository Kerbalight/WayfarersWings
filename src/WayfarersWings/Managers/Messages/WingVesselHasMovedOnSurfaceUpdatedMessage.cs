using KSP.Sim.impl;

namespace WayfarersWings.Managers.Messages;

public class WingVesselHasMovedOnSurfaceUpdatedMessage : WingVesselUpdatedMessage
{
    public bool HasMovedOnSurface { get; }

    public WingVesselHasMovedOnSurfaceUpdatedMessage(VesselComponent vessel, bool hasMovedOnSurface) : base(vessel)
    {
        HasMovedOnSurface = hasMovedOnSurface;
    }
}