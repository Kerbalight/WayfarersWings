using BepInEx.Logging;
using KSP.Messages;
using WayfarersWings.Managers;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Conditions.Events;

public class ConditionEventsRegistry
{
    private static ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("ConditionEventsRegistry");

    public void SetupListeners()
    {
        MessageListener.Instance.MessageCenter.PersistentSubscribe<SOIEnteredMessage>(OnSOIEnteredMessage);
    }

    private void OnSOIEnteredMessage(MessageCenterMessage message)
    {
        var soiMessage = (SOIEnteredMessage)message;
        var transaction = new Transaction(soiMessage, soiMessage.vessel);
        AchievementsOrchestrator.Instance.DispatchTransaction(transaction);
    }
}