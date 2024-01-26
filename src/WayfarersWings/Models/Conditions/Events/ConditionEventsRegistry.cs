using BepInEx.Logging;
using KSP;
using KSP.Game;
using KSP.Messages;
using KSP.Sim.impl;
using WayfarersWings.Managers;
using WayfarersWings.Managers.Messages;
using WayfarersWings.Managers.Observer;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Conditions.Events;

public class ConditionEventsRegistry
{
    private static ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("ConditionEventsRegistry");

    private static MessageCenter MessageCenter => MessageListener.Instance.MessageCenter;

    public void SetupListeners()
    {
        // Orbit
        MessageCenter.PersistentSubscribe<SOIEnteredMessage>(OnSOIEnteredMessage);
        MessageCenter.PersistentSubscribe<StableOrbitCreatedMessage>(OnStableOrbitCreatedMessage);

        // Vessel
        MessageCenter.PersistentSubscribe<VesselLaunchedMessage>(OnVesselLaunched);
        MessageCenter.PersistentSubscribe<VesselSituationChangedMessage>(OnVesselSituationChangedMessage);
        MessageCenter.PersistentSubscribe<VesselLandedGroundAtRestMessage>(OnVesselLandedGroundAtRestMessage);
        MessageCenter.PersistentSubscribe<VesselLandedWaterAtRestMessage>(OnVesselLandedWaterAtRestMessage);
        MessageCenter.PersistentSubscribe<VesselRecoveredMessage>(OnVesselRecovered);

        // Vessel Observer
        MessageCenter.PersistentSubscribe<WingVesselGeeForceUpdatedMessage>(OnVesselGeeForceUpdatedMessage);

        // EVA
        MessageCenter.PersistentSubscribe<EVAEnteredMessage>(OnEVAEnteredMessage);
        MessageCenter.PersistentSubscribe<EVALeftMessage>(OnEVALeftMessage);
        MessageCenter.PersistentSubscribe<FlagPlantedMessage>(OnFlagPlantedMessage);
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
        AchievementsOrchestrator.Instance.DispatchTransaction(transaction);
    }

    /// <summary>
    /// Message does not contain the vessel, we assume it's the active vessel
    /// because the orbit changed right now
    /// </summary>
    public void OnStableOrbitCreatedMessage(MessageCenterMessage message)
    {
        var stableOrbitMessage = (StableOrbitCreatedMessage)message;
        var transaction = ActiveVesselTransaction(stableOrbitMessage);
        AchievementsOrchestrator.Instance.DispatchTransaction(transaction);
    }

    public void OnVesselSituationChangedMessage(MessageCenterMessage message)
    {
        var situationMessage = (VesselSituationChangedMessage)message;

        VesselsStateObserver.Instance.GetVesselObservedState(situationMessage.Vessel).previousSituation =
            situationMessage.OldSituation;

        var transaction = new Transaction(situationMessage, situationMessage.Vessel);
        AchievementsOrchestrator.Instance.DispatchTransaction(transaction);
    }

    public void OnVesselLandedGroundAtRestMessage(MessageCenterMessage message)
    {
        var landedMessage = (VesselLandedGroundAtRestMessage)message;
        var transaction = new Transaction(landedMessage, landedMessage.Vessel);
        AchievementsOrchestrator.Instance.DispatchTransaction(transaction);
    }

    public void OnVesselLandedWaterAtRestMessage(MessageCenterMessage message)
    {
        var landedMessage = (VesselLandedWaterAtRestMessage)message;
        var transaction = new Transaction(landedMessage, landedMessage.Vessel);
        AchievementsOrchestrator.Instance.DispatchTransaction(transaction);
    }

    private static void OnEVAEnteredMessage(MessageCenterMessage message)
    {
        var evaMessage = (EVAEnteredMessage)message;
        var kerbalVessel = KerbalStateObserver.GetActiveOrPendingKerbalVessel();
        var transaction = new Transaction(evaMessage, kerbalVessel);

        // Start tracking kerbal eva time
        KerbalStateObserver.OnEVAEnterMessage(evaMessage, transaction.Vessel);

        AchievementsOrchestrator.Instance.DispatchTransaction(transaction);
    }

    private static void OnEVALeftMessage(MessageCenterMessage message)
    {
        var evaMessage = (EVALeftMessage)message;

        // We need to get the previous vessel because the boarded vessel is the active one
        var transaction = new Transaction(evaMessage, VesselsStateObserver.Instance.PreviousVessel);

        // Update kerbal eva time
        KerbalStateObserver.OnEVALeftMessage(evaMessage, transaction.Vessel);

        AchievementsOrchestrator.Instance.DispatchTransaction(transaction);
    }

    private static void OnVesselGeeForceUpdatedMessage(MessageCenterMessage message)
    {
        var geeForceMessage = (WingVesselGeeForceUpdatedMessage)message;
        var transaction = new Transaction(geeForceMessage, geeForceMessage.Vessel);
        AchievementsOrchestrator.Instance.DispatchTransaction(transaction);
    }

    private static void OnFlagPlantedMessage(MessageCenterMessage message)
    {
        var flagPlantedMessage = (FlagPlantedMessage)message;
        var kerbals = KerbalStateObserver.GetKerbalsInRange();
        var transaction = ActiveVesselTransaction(flagPlantedMessage);
        // Flag planting is a special case, we want to trigger the wing for
        // all nearby kerbals
        transaction.NearbyKerbals.AddRange(kerbals);
        AchievementsOrchestrator.Instance.DispatchTransaction(transaction);
    }

    #region Vessel

    private static void OnVesselLaunched(MessageCenterMessage message)
    {
        var launchedMessage = (VesselLaunchedMessage)message;

        KerbalStateObserver.OnVesselLaunched(launchedMessage, launchedMessage.Vessel);

        var transaction = new Transaction(launchedMessage, launchedMessage.Vessel);
        AchievementsOrchestrator.Instance.DispatchTransaction(transaction);
    }


    private void OnVesselRecovered(MessageCenterMessage message)
    {
        var recoveredMessage = (VesselRecoveredMessage)message;
        var vessel = Core.UniverseModel?.FindVesselComponent(recoveredMessage.VesselID);

        // Update kerbal missions count
        KerbalStateObserver.OnVesselRecovered(recoveredMessage, vessel);

        var transaction = new Transaction(recoveredMessage, vessel);
        AchievementsOrchestrator.Instance.DispatchTransaction(transaction);
    }

    #endregion
}