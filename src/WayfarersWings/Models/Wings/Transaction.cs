using BepInEx.Logging;
using KSP.Game;
using KSP.Messages;
using KSP.Sim.impl;
using UnityEngine;
using WayfarersWings.Managers.Observer;
using WayfarersWings.Models.Session;

namespace WayfarersWings.Models.Wings;

public class Transaction
{
    private static readonly ManualLogSource Logger =
        BepInEx.Logging.Logger.CreateLogSource("WayfarersWings.Transaction");

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
    public List<KerbalInfo> NearbyKerbals { get; set; } = new();

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

    /// <summary>
    /// Gets all kerbals affected by this transaction, depending on how it was created.
    /// </summary>
    public IEnumerable<KerbalInfo> GetKerbals()
    {
        var kerbals = new List<KerbalInfo>();
        
        if (VesselID != null)
            kerbals.AddRange(WingsSessionManager.Roster.GetAllKerbalsInVessel(VesselID.Value));
        if (KerbalInfo != null)
            kerbals.Add(KerbalInfo);
        if (NearbyKerbals.Count > 0)
            kerbals.AddRange(NearbyKerbals);

        if (kerbals.Count == 0)
            Logger.LogWarning("No kerbal in this transaction" + Message?.GetType().Name);

        return kerbals;
    }
}