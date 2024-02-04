using BepInEx.Logging;
using KSP.Game;
using KSP.Messages;
using KSP.Sim.impl;
using WayfarersWings.Managers.Messages;
using WayfarersWings.Models.Session;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Managers.Observer;

public class KerbalStateObserver
{
    private readonly static ManualLogSource Logger =
        BepInEx.Logging.Logger.CreateLogSource("WayfarersWings.KerbalStateObserver");


    #region Vessel

    /// <summary>
    /// For each kerbal in the vessel, run the given action on their profile and then
    /// dispatch a <see cref="WingKerbalProfileUpdatedMessage"/> to notify the UI.
    /// Furthermore, dispatch a <see cref="Transaction"/> to the Orchestrator so that
    /// any wing that depends on the kerbal profile can be triggered.
    /// </summary>
    private static void RunAndDispatchForKerbalsInVessel(MessageCenterMessage message, VesselComponent? vesselComponent,
        Func<KerbalProfile, bool> runOnProfile)
    {
        if (vesselComponent == null) return;
        var kerbals = WingsSessionManager.Roster.GetAllKerbalsInVessel(vesselComponent.GlobalId);
        foreach (var kerbal in kerbals)
        {
            var profile = WingsSessionManager.Instance.GetKerbalProfile(kerbal.Id);
            var hasChanged = runOnProfile(profile);
            if (!hasChanged) continue;

            Core.Messages.Publish(new WingKerbalProfileUpdatedMessage(profile));

            var transaction = new Transaction(message, kerbal);
            AchievementsOrchestrator.DispatchTransaction(transaction);
        }
    }

    public static void OnLaunchFromVABMessage(LaunchFromVABMessage message, VesselComponent? vesselComponent)
    {
        RunAndDispatchForKerbalsInVessel(
            message,
            vesselComponent,
            profile =>
            {
                profile.StartMission(vesselComponent!);
                return true;
            });
    }

    public static void OnVesselLaunched(VesselLaunchedMessage message, VesselComponent? vesselComponent)
    {
        RunAndDispatchForKerbalsInVessel(
            message,
            vesselComponent,
            profile =>
            {
                profile.UpdateOnVesselLaunched(vesselComponent!);
                return true;
            });
    }

    public static void OnVesselRecovered(VesselRecoveredMessage message, VesselComponent? vesselComponent)
    {
        RunAndDispatchForKerbalsInVessel(
            message,
            vesselComponent,
            profile =>
            {
                if (profile.IsInEVA) profile.CompleteEVA(vesselComponent!);
                profile.CompleteMission(vesselComponent!);
                return true;
            });
    }

    #endregion

    // For the following events, we don't need to dispatch a transaction to the Orchestrator,
    // since they are already KerbalComponent, so they are processed in the main
    // transaction (triggered in ConditionEventsRegistry.cs)

    #region EVA

    public static void OnEVAEnterMessage(EVAEnteredMessage message, Transaction transaction)
    {
        if (transaction.KerbalProfile == null || transaction.Vessel == null)
        {
            Logger.LogWarning($"KerbalProfile or Vessel is null for EVAEnteredMessage, transaction: {transaction}");
            return;
        }

        transaction.KerbalProfile.StartEVA(transaction.Vessel);
        Core.Messages.Publish(new WingKerbalProfileUpdatedMessage(transaction.KerbalProfile));
    }

    public static void OnEVALeftMessage(EVALeftMessage message, Transaction transaction)
    {
        if (transaction.KerbalProfile == null || transaction.Vessel == null)
        {
            Logger.LogWarning($"KerbalProfile or Vessel is null for EVALeftMessage, transaction: {transaction}");
            return;
        }

        transaction.KerbalProfile.CompleteEVA(transaction.Vessel);
        Core.Messages.Publish(new WingKerbalProfileUpdatedMessage(transaction.KerbalProfile));
    }

    #endregion

    #region Science

    /// <summary>
    /// Update the KerbalProfile for all kerbals in the vessel. Since this
    /// message is triggered at high frequency, we want to use hasChanged to
    /// prevent unnecessary Transaction dispatches.
    /// </summary>
    public static void OnVesselScienceSituationChanged(VesselScienceSituationChangedMessage message,
        VesselComponent? vesselComponent)
    {
        RunAndDispatchForKerbalsInVessel(
            message,
            vesselComponent,
            profile =>
            {
                profile.OnScienceSituationChange(message.NewSituation, out var hasChanged);
                return hasChanged;
            });
    }

    #endregion
}