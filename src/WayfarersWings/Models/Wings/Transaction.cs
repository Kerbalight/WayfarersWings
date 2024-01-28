using BepInEx.Logging;
using KSP.Game;
using KSP.Messages;
using KSP.Sim.impl;
using UnityEngine;
using WayfarersWings.Managers.Observer;
using WayfarersWings.Models.Session;
using WayfarersWings.Utility;

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
        Message = message;
        VesselID = vessel?.GlobalId;
        SimulationObject = vessel?.SimulationObject;
        Vessel = vessel;
        ObservedState = VesselsStateObserver.Instance.GetVesselObservedState(VesselID);

        // If the vessel is a kerbal (EVA), we can get the KerbalInfo and KerbalProfile
        // in order to process the transaction.
        if (vessel?.SimulationObject?.Kerbal != null)
        {
            KerbalInfo = vessel.SimulationObject.Kerbal.GetAndAssignIfNeededKerbalInfo();
            if (KerbalInfo != null)
                KerbalProfile = WingsSessionManager.Instance.GetKerbalProfile(KerbalInfo.Id);
            else
                Logger.LogWarning($"KerbalInfo is null for KerbalComponent vessel: {vessel}");
        }
    }

    /// <summary>
    /// This constructor is used when the transaction is not related to a vessel,
    /// but only to a kerbal profile.
    /// </summary>
    public Transaction(MessageCenterMessage? message, KerbalInfo kerbalInfo)
    {
        Message = message;
        KerbalInfo = kerbalInfo;
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
            Logger.LogWarning($"No kerbal in this transaction {Message?.GetType().Name}");

        return kerbals;
    }
}