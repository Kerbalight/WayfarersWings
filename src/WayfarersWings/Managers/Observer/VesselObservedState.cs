using KSP.Sim.impl;
using WayfarersWings.Managers.Messages;
using WayfarersWings.Managers.Observer.Properties;

namespace WayfarersWings.Managers.Observer;

public class VesselObservedState
{
    public CelestialBodyComponent? referenceBody;
    public VesselObservedGeeForce geeForce = new();

    public IEnumerable<WingVesselUpdatedMessage> Update(VesselComponent vessel, out bool hasChanged)
    {
        var triggeredMessages = new List<WingVesselUpdatedMessage>();
        referenceBody = vessel.mainBody;

        if (geeForce.Update(vessel, out var geeForceMessage)) triggeredMessages.Add(geeForceMessage!);

        hasChanged = triggeredMessages.Count > 0;
        return triggeredMessages;
    }
}