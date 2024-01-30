using KSP.Sim.impl;
using WayfarersWings.Managers.Messages;
using WayfarersWings.Managers.Observer.Properties;

namespace WayfarersWings.Managers.Observer;

public class VesselObservedState
{
    // Tracked properties
    public CelestialBodyComponent? referenceBody;
    public VesselSituations? previousSituation;

    public readonly VesselObservedGeeForce geeForce = new();
    public readonly VesselObservedHasMovedOnSurface hasMovedOnSurface = new();

    public void Start(VesselComponent vessel)
    {
        geeForce.Start(vessel);
        hasMovedOnSurface.Start(vessel);
    }

    public IEnumerable<WingVesselUpdatedMessage> Update(VesselComponent vessel, out bool hasChanged)
    {
        var triggeredMessages = new List<WingVesselUpdatedMessage>();
        referenceBody = vessel.mainBody;

        if (geeForce.Update(vessel, out var geeForceMessage))
            triggeredMessages.Add(geeForceMessage!);
        if (hasMovedOnSurface.Update(vessel, out var hasMovedOnSurfaceMessage))
            triggeredMessages.Add(hasMovedOnSurfaceMessage!);

        hasChanged = triggeredMessages.Count > 0;
        return triggeredMessages;
    }
}