using KSP.Game;
using KSP.Messages;
using KSP.Sim.impl;
using WayfarersWings.Models.Session;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Managers.Observer;

public class KerbalStateObserver
{
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

    public void OnEVAEnterMessage(EVAEnteredMessage message, VesselComponent? vesselComponent)
    {
        if (vesselComponent == null) return;
        var kerbal = vesselComponent.SimulationObject.Kerbal.KerbalInfo;
        var profile = WingsSessionManager.Instance.GetKerbalProfile(kerbal.Id);
        profile.lastEvaEnteredAt = Core.GetUniverseTime();

        var transaction = new Transaction(message, kerbal);
        AchievementsOrchestrator.Instance.DispatchTransaction(transaction);
    }

    public static void OnEVALeftMessage(EVALeftMessage message, VesselComponent? vesselComponent)
    {
        if (vesselComponent == null) return;
        var kerbal = vesselComponent.SimulationObject.Kerbal.KerbalInfo;
        var profile = WingsSessionManager.Instance.GetKerbalProfile(kerbal.Id);
        profile.CompleteEVA(vesselComponent);

        var transaction = new Transaction(message, kerbal);
        AchievementsOrchestrator.Instance.DispatchTransaction(transaction);
    }
}