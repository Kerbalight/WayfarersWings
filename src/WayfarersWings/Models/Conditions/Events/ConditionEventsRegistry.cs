using BepInEx.Logging;
using KSP;
using KSP.Game;
using KSP.Messages;
using KSP.Sim.impl;
using WayfarersWings.Managers;
using WayfarersWings.Managers.Messages;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Conditions.Events;

public class ConditionEventsRegistry
{
    private static ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("ConditionEventsRegistry");

    public void SetupListeners()
    {
        MessageListener.Instance.MessageCenter.PersistentSubscribe<SOIEnteredMessage>(OnSOIEnteredMessage);
        MessageListener.Instance.MessageCenter.PersistentSubscribe<StableOrbitCreatedMessage>(
            OnStableOrbitCreatedMessage);
        MessageListener.Instance.MessageCenter.PersistentSubscribe<EVAEnteredMessage>(OnEVAEnteredMessage);
        MessageListener.Instance.MessageCenter.PersistentSubscribe<VesselSituationChangedMessage>(
            OnVesselSituationChangedMessage);
        MessageListener.Instance.MessageCenter.PersistentSubscribe<VesselLandedGroundAtRestMessage>(
            OnVesselLandedGroundAtRestMessage);
        MessageListener.Instance.MessageCenter.PersistentSubscribe<VesselLandedWaterAtRestMessage>(
            OnVesselLandedWaterAtRestMessage);
        MessageListener.Instance.MessageCenter.PersistentSubscribe<WingVesselGeeForceUpdatedMessage>(
            OnVesselGeeForceUpdatedMessage);
    }

    private void OnSOIEnteredMessage(MessageCenterMessage message)
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

        // TODO
        var vessels = GameManager.Instance.Game.SpaceSimulation.GetAllObjectsWithComponent<OrbiterComponent>();

        var transaction = new Transaction(stableOrbitMessage,
            GameManager.Instance.Game.ViewController.GetActiveSimVessel());
        AchievementsOrchestrator.Instance.DispatchTransaction(transaction);
    }

    public void OnVesselSituationChangedMessage(MessageCenterMessage message)
    {
        var situationMessage = (VesselSituationChangedMessage)message;
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

    public void OnEVAEnteredMessage(MessageCenterMessage message)
    {
        var evaMessage = (EVAEnteredMessage)message;
        var allVessels = GameManager.Instance.Game.UniverseModel.GetAllVessels();
        var allKerbals =
            GameManager.Instance.Game.SpaceSimulation.GetAllSimulationObjectsWithComponent<KerbalComponent>();
        foreach (var kerbalSimObj in allKerbals)
        {
            if (!kerbalSimObj.IsKerbal) continue;
            var transaction = new Transaction(evaMessage, kerbalSimObj.Vessel);
            AchievementsOrchestrator.Instance.DispatchTransaction(transaction);
        }
    }

    public void OnVesselGeeForceUpdatedMessage(MessageCenterMessage message)
    {
        var geeForceMessage = (WingVesselGeeForceUpdatedMessage)message;
        var transaction = new Transaction(geeForceMessage, geeForceMessage.Vessel);
        AchievementsOrchestrator.Instance.DispatchTransaction(transaction);
    }
}