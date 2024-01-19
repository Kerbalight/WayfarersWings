using BepInEx.Logging;
using KSP;
using KSP.Game;
using KSP.Messages;
using KSP.Sim.impl;
using WayfarersWings.Managers;
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

    public void OnEVAEnteredMessage(MessageCenterMessage message)
    {
        var evaMessage = (EVAEnteredMessage)message;
        var active = GameManager.Instance.Game.ViewController.GetActiveSimVessel();
        var transaction = new Transaction(evaMessage, active);
        AchievementsOrchestrator.Instance.DispatchTransaction(transaction);
    }
}