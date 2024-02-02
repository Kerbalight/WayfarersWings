using KSP.Sim.impl;

namespace WayfarersWings.Managers.Observer.Properties.Types;

/// <summary>
/// Some properties are subject to fluctuations, and we want to keep track of the minimum value
/// during a certain period of time. This class provides a way to do that, avoiding
/// issues like Gee Force spikes.
/// </summary>
public abstract class VesselObservedSustainedProperty : VesselObservedProperty<int>
{
    private const int WindowDuration = 3; // 3 seconds
    private int[] _history = new int[WindowDuration];

    protected override int GetValue(VesselComponent vessel)
    {
        var nextHistory = new int[WindowDuration];
        Array.Copy(_history, 1, nextHistory, 0, WindowDuration - 1);
        _history = nextHistory;
        var instantValue = GetInstantValue(vessel);
        _history[WindowDuration - 1] = instantValue;


        var sustained = instantValue;
        for (var i = 0; i < WindowDuration; i++)
        {
            if (_history[i] < sustained) sustained = _history[i];
        }

        return sustained;
    }

    /// <summary>
    /// Get the instantaneous value of the property, that will be used to provide
    /// the minimum value of the property over the last 5 seconds (sustained value)
    /// </summary>
    protected abstract int GetInstantValue(VesselComponent vessel);
}