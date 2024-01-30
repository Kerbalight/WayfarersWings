using BepInEx.Logging;
using KSP;
using KSP.Game;
using KSP.Messages;
using KSP.Sim.impl;
using WayfarersWings.Managers;
using WayfarersWings.Managers.Messages;
using WayfarersWings.Managers.Observer;
using WayfarersWings.Models.Session;
using WayfarersWings.Models.Wings;
using WayfarersWings.Utility;

namespace WayfarersWings.Models.Conditions.Events;

public class ConditionEventsRegistry
{
    private static ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("ConditionEventsRegistry");

    private static MessageCenter Messages => MessageListener.MessageCenter;

    public void SetupListeners()
    {
        // Orbit
        Messages.PersistentSubscribe<SOIEnteredMessage>(OnSOIEnteredMessage);
        Messages.PersistentSubscribe<StableOrbitCreatedMessage>(OnStableOrbitCreatedMessage);

        // Vessel
        Messages.PersistentSubscribe<LaunchFromVABMessage>(OnLaunchFromVABMessage);
        Messages.PersistentSubscribe<VesselLaunchedMessage>(OnVesselLaunched);
        Messages.PersistentSubscribe<VesselSituationChangedMessage>(OnVesselSituationChangedMessage);
        Messages.PersistentSubscribe<VesselLandedGroundAtRestMessage>(OnVesselLandedGroundAtRestMessage);
        Messages.PersistentSubscribe<VesselLandedWaterAtRestMessage>(OnVesselLandedWaterAtRestMessage);
        Messages.PersistentSubscribe<VesselRecoveredMessage>(OnVesselRecovered);
        Messages.PersistentSubscribe<VesselDockedMessage>(OnVesselDocked);

        // Vessel Observer
        Messages.PersistentSubscribe<WingVesselGeeForceUpdatedMessage>(OnVesselGeeForceUpdatedMessage);

        // EVA
        Messages.PersistentSubscribe<EVAEnteredMessage>(OnEVAEnteredMessage);
        Messages.PersistentSubscribe<EVALeftMessage>(OnEVALeftMessage);
        Messages.PersistentSubscribe<FlagPlantedMessage>(OnFlagPlantedMessage);

        // Science
        // TODO implement this
        // Messages.PersistentSubscribe<ResearchReportScoredMessage>(OnResearchReportScoredMessage);
    }

    private static Transaction ActiveVesselTransaction(MessageCenterMessage message)
    {
        var activeVessel = GameManager.Instance.Game.ViewController.GetActiveSimVessel();
        return new Transaction(message, activeVessel);
    }

    private static void OnSOIEnteredMessage(MessageCenterMessage message)
    {
        var soiMessage = (SOIEnteredMessage)message;
        var transaction = new Transaction(soiMessage, soiMessage.vessel);
        AchievementsOrchestrator.DispatchTransaction(transaction);
    }

    /// <summary>
    /// Message does not contain the vessel, we assume it's the active vessel
    /// because the orbit changed right now
    /// </summary>
    private static void OnStableOrbitCreatedMessage(MessageCenterMessage message)
    {
        var stableOrbitMessage = (StableOrbitCreatedMessage)message;
        var transaction = ActiveVesselTransaction(stableOrbitMessage);
        AchievementsOrchestrator.DispatchTransaction(transaction);
    }

    public static void OnVesselSituationChangedMessage(MessageCenterMessage message)
    {
        var situationMessage = (VesselSituationChangedMessage)message;

        VesselsStateObserver.Instance.GetVesselObservedState(situationMessage.Vessel).previousSituation =
            situationMessage.OldSituation;

        var transaction = new Transaction(situationMessage, situationMessage.Vessel);
        AchievementsOrchestrator.DispatchTransaction(transaction);
    }

    public static void OnVesselLandedGroundAtRestMessage(MessageCenterMessage message)
    {
        var landedMessage = (VesselLandedGroundAtRestMessage)message;
        var transaction = new Transaction(landedMessage, landedMessage.Vessel);
        AchievementsOrchestrator.DispatchTransaction(transaction);
    }

    public static void OnVesselLandedWaterAtRestMessage(MessageCenterMessage message)
    {
        var landedMessage = (VesselLandedWaterAtRestMessage)message;
        var transaction = new Transaction(landedMessage, landedMessage.Vessel);
        AchievementsOrchestrator.DispatchTransaction(transaction);
    }

    private static void OnEVAEnteredMessage(MessageCenterMessage message)
    {
        var evaMessage = (EVAEnteredMessage)message;
        var kerbalVessel = KerbalUtils.GetActiveOrPendingKerbalVessel();
        var transaction = new Transaction(evaMessage, kerbalVessel);

        // Start tracking kerbal eva time
        KerbalStateObserver.OnEVAEnterMessage(evaMessage, transaction);

        AchievementsOrchestrator.DispatchTransaction(transaction);
    }

    private static void OnEVALeftMessage(MessageCenterMessage message)
    {
        var evaMessage = (EVALeftMessage)message;

        // We need to get the previous vessel (the Kerbal) because the
        // boarded vessel is the Active one
        var transaction = new Transaction(evaMessage, VesselsStateObserver.Instance.PreviousVessel);

        // Update kerbal eva time
        KerbalStateObserver.OnEVALeftMessage(evaMessage, transaction);

        AchievementsOrchestrator.DispatchTransaction(transaction);
    }

    private static void OnVesselGeeForceUpdatedMessage(MessageCenterMessage message)
    {
        var geeForceMessage = (WingVesselGeeForceUpdatedMessage)message;
        var transaction = new Transaction(geeForceMessage, geeForceMessage.Vessel);
        AchievementsOrchestrator.DispatchTransaction(transaction);
    }

    private static void OnFlagPlantedMessage(MessageCenterMessage message)
    {
        var flagPlantedMessage = (FlagPlantedMessage)message;
        var kerbals = KerbalUtils.GetKerbalsInRange();
        var transaction = ActiveVesselTransaction(flagPlantedMessage);
        // Flag planting is a special case, we want to trigger the wing for
        // all nearby kerbals
        transaction.NearbyKerbals.AddRange(kerbals);
        AchievementsOrchestrator.DispatchTransaction(transaction);
    }

    #region Vessel

    private static void OnLaunchFromVABMessage(MessageCenterMessage message)
    {
        var launchedMessage = (LaunchFromVABMessage)message;
        var vessel = GameManager.Instance.Game.ViewController.GetPendingActiveVessel();

        KerbalStateObserver.OnLaunchFromVABMessage(launchedMessage, vessel);

        var transaction = new Transaction(launchedMessage, vessel);
        AchievementsOrchestrator.DispatchTransaction(transaction);
    }

    private static void OnVesselLaunched(MessageCenterMessage message)
    {
        var launchedMessage = (VesselLaunchedMessage)message;

        KerbalStateObserver.OnVesselLaunched(launchedMessage, launchedMessage.Vessel);

        var transaction = new Transaction(launchedMessage, launchedMessage.Vessel);
        AchievementsOrchestrator.DispatchTransaction(transaction);
    }

    private static void OnVesselRecovered(MessageCenterMessage message)
    {
        var recoveredMessage = (VesselRecoveredMessage)message;
        var vessel = Core.UniverseModel?.FindVesselComponent(recoveredMessage.VesselID);

        // Update kerbal missions count
        KerbalStateObserver.OnVesselRecovered(recoveredMessage, vessel);

        var transaction = new Transaction(recoveredMessage, vessel);
        AchievementsOrchestrator.DispatchTransaction(transaction);

        // Show flight report summary
        Messages.Publish(new WingMissionCompletedMessage(vessel));
    }

    private static void OnVesselDocked(MessageCenterMessage message)
    {
        var dockedMessage = (VesselDockedMessage)message;
        var dockerVesselId = dockedMessage.VesselOne.SimObjectComponent.SimulationObject.GlobalId;
        var dockerVessel = Core.UniverseModel?.FindVesselComponent(dockerVesselId);

        var dockeeVesselId = dockedMessage.VesselTwo.SimObjectComponent.SimulationObject.GlobalId;
        // var dockeeVessel = Core.UniverseModel?.FindVesselComponent(dockeeVesselId);

        var transaction = new Transaction(dockedMessage, dockerVessel);
        transaction.NearbyKerbals.AddRange(WingsSessionManager.Roster.GetAllKerbalsInVessel(dockeeVesselId));
        AchievementsOrchestrator.DispatchTransaction(transaction);
    }

    #endregion

    #region Science

    // private static void OnResearchReportScoredMessage(MessageCenterMessage message)
    // {
    //     var scoredMessage = (ResearchReportScoredMessage)message;
    //     scoredMessage.ResearchReportKey;
    // }

    #endregion
}