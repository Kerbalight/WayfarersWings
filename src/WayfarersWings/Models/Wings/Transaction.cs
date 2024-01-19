using KSP.Game;
using KSP.Messages;
using KSP.Sim.impl;
using UnityEngine;

namespace WayfarersWings.Models.Wings;

public class Transaction
{
    public MessageCenterMessage? Message { get; set; }
    public IGGuid? VesselID { get; set; }
    public SimulationObjectModel? SimulationObject { get; set; }
    public VesselComponent? Vessel { get; set; }

    public Transaction(MessageCenterMessage? message, VesselComponent? vessel)
    {
        VesselID = vessel?.GlobalId;
        SimulationObject = vessel?.SimulationObject;
        Message = message;
        Vessel = vessel;
    }

    public List<KerbalInfo> GetKerbals()
    {
        var vesselID = VesselID;
        if (vesselID == null)
        {
            Debug.LogError("VesselID is null");
            return [];
        }

        return GameManager.Instance.Game.SessionManager.KerbalRosterManager.GetAllKerbalsInVessel(vesselID.Value);
    }
}