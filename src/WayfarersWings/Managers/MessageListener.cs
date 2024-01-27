using KSP.Game;
using KSP.Messages;
using WayfarersWings.Models.Session;
using WayfarersWings.UI;


namespace WayfarersWings.Managers;

public class MessageListener
{
    public static MessageListener Instance { get; } = new();

    public static MessageCenter MessageCenter => GameManager.Instance.Game.Messages;

    /// <summary>
    /// Subscribe to messages from the game, without blocking for the needed delay.
    /// </summary>
    public void SubscribeToMessages()
    {
        _ = Subscribe();
    }

    private static async Task Subscribe()
    {
        await Task.Delay(100);

        MessageCenter.PersistentSubscribe<GameLoadFinishedMessage>(OnGameLoadFinishedMessage);
        MessageCenter.PersistentSubscribe<GameStateChangedMessage>(HideWindowOnInvalidState);

        Core.Instance.EventsRegistry.SetupListeners();
    }

    public SubscriptionHandle Subscribe<TMessage>(Action<MessageCenterMessage> callback)
        where TMessage : MessageCenterMessage
    {
        return MessageCenter.PersistentSubscribe<TMessage>(callback);
    }

    private static void OnGameLoadFinishedMessage(MessageCenterMessage message)
    {
        // Load Wings from WingsConfig
        Core.Instance.WingsPool.LoadRegisteredConfigs();
        // Load GameData
        SaveManager.Instance.LoadGameDataInSession();
    }

    private static void HideWindowOnInvalidState(MessageCenterMessage message)
    {
        if (GameStateManager.IsInvalidState())
        {
            // Close the windows if the game is in an invalid state
            MainUIManager.Instance.AppWindow.IsWindowOpen = false;
            MainUIManager.Instance.MissionSummaryWindow.IsWindowOpen = false;
        }

        if (!GameStateManager.CanShowFlightReport())
        {
            MainUIManager.Instance.MissionSummaryWindow.IsWindowOpen = false;
        }
    }
}