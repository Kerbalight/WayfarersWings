using KSP.Sim.impl;

namespace WayfarersWings.Managers.Observer;

public class VesselsStateObserver
{
    private Dictionary<IGGuid, VesselObservedState> _vesselsState = new();

    public void UpdateVessel(VesselComponent vessel)
    {
        var vesselId = vessel.SimulationObject.GlobalId;
        if (!_vesselsState.TryGetValue(vessel.SimulationObject.GlobalId, out var state))
        {
            state = new VesselObservedState();
            // _vesselsState.Add(vessel.Guid, state);
        }
    }
}