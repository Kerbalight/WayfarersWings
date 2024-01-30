using KSP.Sim.impl;
using WayfarersWings.Managers.Messages;
using WayfarersWings.Managers.Observer.Properties.Events;

namespace WayfarersWings.Managers.Observer.Properties;

/// <summary>
/// Atmospheric density, rounded to int
/// </summary>
public class VesselObservedAtmDensity : VesselObservedProperty<int>
{
    public override int GetValue(VesselComponent vessel)
    {
        return (int)vessel.AtmDensity;
    }

    protected override WingVesselUpdatedMessage CreateTriggeredMessage(VesselComponent vessel)
    {
        return new WingVesselAtmDensityUpdatedMessage(vessel, Value);
    }
}