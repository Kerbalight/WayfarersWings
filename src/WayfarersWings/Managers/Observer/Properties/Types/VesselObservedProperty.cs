using KSP.Sim.impl;
using WayfarersWings.Managers.Observer.Properties.Events;

namespace WayfarersWings.Managers.Observer.Properties.Types;

public abstract class VesselObservedProperty<T>
{
    public T? Value { get; private set; }
    public T? PreviousValue { get; private set; }

    public virtual bool HasChanged => !Value?.Equals(PreviousValue) ?? false;

    protected abstract T GetValue(VesselComponent vessel);

    /// <summary>
    /// Can be used to initialize the property, called when the vessel is first
    /// observed or when it's changed.
    /// </summary>
    public virtual void Start(VesselComponent vessel) { }

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

    protected abstract WingVesselUpdatedMessage CreateTriggeredMessage(VesselComponent vessel);
}