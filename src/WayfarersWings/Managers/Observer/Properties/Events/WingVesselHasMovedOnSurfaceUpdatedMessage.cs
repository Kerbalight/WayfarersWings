using KSP.Sim.impl;
using WayfarersWings.Managers.Messages;

namespace WayfarersWings.Managers.Observer.Properties.Events;

public class WingVesselHasMovedOnSurfaceUpdatedMessage : WingVesselUpdatedMessage
{
    public bool HasMovedOnSurface { get; }

    public WingVesselHasMovedOnSurfaceUpdatedMessage(VesselComponent vessel, bool hasMovedOnSurface) : base(vessel)
    {
        HasMovedOnSurface = hasMovedOnSurface;
    }
}