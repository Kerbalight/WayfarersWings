using KSP.Game;
using KSP.Messages;
using KSP.Sim.impl;
using UnityEngine;
using WayfarersWings.Managers.Observer;
using WayfarersWings.Models.Session;

namespace WayfarersWings.Models.Wings;

public class Transaction
{
    public MessageCenterMessage? Message { get; set; }
    public IGGuid? VesselID { get; set; }
    public SimulationObjectModel? SimulationObject { get; set; }
    public VesselComponent? Vessel { get; set; }
    public KerbalInfo? KerbalInfo { get; set; }
    public KerbalProfile? KerbalProfile { get; set; }
    public VesselObservedState? ObservedState { get; private set; }

    /// <summary>
    /// Promotes this transaction to a transaction that affects
    /// all nearby kerbals.
    /// Used for example for flag planting.
    /// </summary>
    public List<KerbalInfo> AffectedKerbals { get; set; } = new();

    public Transaction(MessageCenterMessage? message, VesselComponent? vessel)
    {
        VesselID = vessel?.GlobalId;
        SimulationObject = vessel?.SimulationObject;
        Message = message;
        Vessel = vessel;
        ObservedState = VesselsStateObserver.Instance.GetVesselObservedState(VesselID);
    }

    public Transaction(MessageCenterMessage? message, KerbalInfo kerbalInfo)
    {
        KerbalInfo = kerbalInfo;
        Message = message;
        KerbalProfile = WingsSessionManager.Instance.GetKerbalProfile(kerbalInfo.Id);
    }

    public IEnumerable<KerbalInfo> GetKerbals()
    {
        var kerbals = new List<KerbalInfo>();
        if (AffectedKerbals.Count > 0)
        {
            kerbals.AddRange(AffectedKerbals);
        }

        var vesselID = VesselID;
        if (vesselID == null)
        {
            Debug.LogError("VesselID is null");
            return kerbals;
        }

        kerbals.AddRange(
            WingsSessionManager.Roster.GetAllKerbalsInVessel(vesselID.Value));
        return kerbals;
    }
}