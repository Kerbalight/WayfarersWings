using BepInEx.Logging;
using KSP.Game;
using KSP.Messages;
using KSP.Sim.impl;
using WayfarersWings.Models.Session;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Managers.Observer;

public class KerbalStateObserver
{
    private readonly static ManualLogSource Logger =
        BepInEx.Logging.Logger.CreateLogSource("WayfarersWings.KerbalStateObserver");

    public static IEnumerable<KerbalInfo> GetKerbalsInRange()
    {
        var kerbals = new List<KerbalInfo>();
        var vesselsInRange = GameManager.Instance.Game.ViewController.VesselsInRange;
        foreach (var vessel in vesselsInRange)
        {
            if (vessel.IsKerbalEVA) kerbals.Add(vessel.SimulationObject.Kerbal.KerbalInfo);
            else if (vessel.IsVesselAtRest() && vessel.LandedOrSplashed)
                kerbals.AddRange(WingsSessionManager.Roster.GetAllKerbalsInVessel(vessel.GlobalId));
        }

        return kerbals;
    }

    /// <summary>
    /// When an "EVA entered" message is dispatched, we are not sure if
    /// the _active_ vessel is the kerbal or it's the _pending_ vessel.
    /// </summary>
    /// <returns></returns>
    public static VesselComponent? GetActiveOrPendingKerbalVessel()
    {
        var pendingVessel = GameManager.Instance.Game.ViewController.GetPendingActiveVessel();
        if (pendingVessel?.IsKerbalEVA == true) return pendingVessel;

        return GameManager.Instance.Game.ViewController.GetActiveSimVessel();
    }

    /// <summary>
    /// KerbalInfo may be null when EVA is started (EVAEnteredMessage)
    /// </summary>
    /// <param name="kerbal"></param>
    /// <returns></returns>
    private static KerbalInfo? GetKerbalInfo(KerbalComponent kerbal)
    {
        if (kerbal.KerbalInfo == null)
        {
            kerbal.AssignKerbalInfo();
        }

        return kerbal.KerbalInfo;
    }

    #region Vessel

    public static void OnVesselLaunched(VesselLaunchedMessage message, VesselComponent? vesselComponent)
    {
        if (vesselComponent == null) return;
        var kerbals = WingsSessionManager.Roster.GetAllKerbalsInVessel(vesselComponent.GlobalId);
        foreach (var kerbal in kerbals)
        {
            var profile = WingsSessionManager.Instance.GetKerbalProfile(kerbal.Id);
            profile.lastLaunchedAt = Core.GetUniverseTime();

            var transaction = new Transaction(message, kerbal);
            AchievementsOrchestrator.Instance.DispatchTransaction(transaction);
        }
    }

    public static void OnVesselRecovered(VesselRecoveredMessage message, VesselComponent? vesselComponent)
    {
        if (vesselComponent == null) return;
        var kerbals = WingsSessionManager.Roster.GetAllKerbalsInVessel(vesselComponent.GlobalId);
        foreach (var kerbal in kerbals)
        {
            var profile = WingsSessionManager.Instance.GetKerbalProfile(kerbal.Id);
            profile.CompleteMission(vesselComponent);

            var transaction = new Transaction(message, kerbal);
            AchievementsOrchestrator.Instance.DispatchTransaction(transaction);
        }
    }

    #endregion

    #region EVA

    public static void OnEVAEnterMessage(EVAEnteredMessage message, VesselComponent? kerbalVessel)
    {
        if (kerbalVessel == null) return;

        var kerbalInfo = GetKerbalInfo(kerbalVessel.SimulationObject.Kerbal);
        if (kerbalInfo == null)
        {
            Logger.LogError("Could not get KerbalInfo from KerbalComponent OnEVAEnter. Aborting transaction");
            return;
        }

        var profile = WingsSessionManager.Instance.GetKerbalProfile(kerbalInfo.Id);
        profile.lastEvaEnteredAt = Core.GetUniverseTime();

        var transaction = new Transaction(message, kerbalInfo);
        AchievementsOrchestrator.Instance.DispatchTransaction(transaction);
    }

    public static void OnEVALeftMessage(EVALeftMessage message, VesselComponent? kerbalVessel)
    {
        if (kerbalVessel == null) return;
        var kerbalInfo = kerbalVessel.SimulationObject.Kerbal.KerbalInfo;
        var profile = WingsSessionManager.Instance.GetKerbalProfile(kerbalInfo.Id);
        profile.CompleteEVA(kerbalVessel);

        var transaction = new Transaction(message, kerbalInfo);
        AchievementsOrchestrator.Instance.DispatchTransaction(transaction);
    }

    #endregion
}