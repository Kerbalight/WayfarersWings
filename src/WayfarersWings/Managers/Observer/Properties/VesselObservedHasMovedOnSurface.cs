using BepInEx.Logging;
using KSP.Modules;
using KSP.Sim.impl;
using Unity.Mathematics;
using WayfarersWings.Managers.Analyzers;
using WayfarersWings.Managers.Messages;

namespace WayfarersWings.Managers.Observer.Properties;

public class VesselObservedHasMovedOnSurface : VesselObservedProperty<bool>
{
    /// <summary>
    /// We require the vessel to move at least 3 meters on the ground
    /// to trigger
    /// </summary>
    private const double MinSquaredGroundDistanceToTrigger = 3.0d * 3.0d;

    private readonly static ManualLogSource Logger =
        BepInEx.Logging.Logger.CreateLogSource("WayfarersWings.VesselObservedHasMovedOnSurface");

    /// <summary>
    /// Keeps a cache of the wheels of the vessel. This is updated when the vessel
    /// is changed.
    /// </summary>
    private List<Data_WheelBase> _cachedWheels = [];

    private bool _hasWheels;

    /// <summary>
    /// If the vessel has moved on the surface, this is the last position of the
    /// vessel on the ground.
    /// </summary>
    private Vector3d _lastGroundPosition = Vector3d.zero;

    public override void Start(VesselComponent vessel)
    {
        _cachedWheels = VesselPartsAnalyzer.GetRoverWheels(vessel);
        _hasWheels = _cachedWheels.Count > 0;
        Logger.LogDebug($"Vessel {vessel.Name} has {_cachedWheels.Count} wheels");
    }

    public override bool GetValue(VesselComponent vessel)
    {
        if (!_hasWheels || vessel.Situation != VesselSituations.Landed) return false;
        foreach (var dataWheel in _cachedWheels)
        {
            if (!dataWheel.IsGrounded) return false;
        }

        if (_lastGroundPosition.IsZero())
        {
            Logger.LogInfo($"Vessel {vessel.Name} detected as a rover on the surface");
            _lastGroundPosition = vessel.transform.localPosition;
            return false;
        }

        // If the vessel is moving, we consider it has moved on the surface, even if it's
        // going back to the starting position.
        if (Value) return true;

        var delta = _lastGroundPosition - vessel.transform.localPosition;
        if (delta.sqrMagnitude > MinSquaredGroundDistanceToTrigger)
        {
            Logger.LogInfo($"Vessel {vessel.Name} rover has moved on the surface");
            return true;
        }

        return false;
    }

    protected override WingVesselUpdatedMessage CreateTriggeredMessage(VesselComponent vessel)
    {
        return new WingVesselHasMovedOnSurfaceUpdatedMessage(vessel, Value);
    }
}