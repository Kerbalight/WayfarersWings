using KSP.Sim.impl;
using WayfarersWings.Managers.Messages;

namespace WayfarersWings.Managers.Observer.Properties;

public abstract class VesselObservedProperty<T>
{
    public T Value { get; private set; }
    public T PreviousValue { get; private set; }

    public bool HasChanged => !Value?.Equals(PreviousValue) ?? false;

    public abstract T GetValue(VesselComponent vessel);

    public bool Update(VesselComponent vessel, out WingVesselUpdatedMessage? message)
    {
        PreviousValue = Value;
        Value = GetValue(vessel);
        var hasChanged = HasChanged;

        if (hasChanged)
        {
            message = CreateTriggeredMessage(vessel);
            return true;
        }

        message = null;
        return false;
    }

    public abstract WingVesselUpdatedMessage CreateTriggeredMessage(VesselComponent vessel);
}