using KSP.Game;
using KSP.Messages;
using WayfarersWings.Models.Session;
using WayfarersWings.UI;


namespace WayfarersWings.Managers;

public class MessageListener
{
    public static MessageListener Instance { get; } = new();

    public MessageCenter MessageCenter => GameManager.Instance.Game.Messages;

    /// <summary>
    /// Subscribe to messages from the game, without blocking for the needed delay.
    /// </summary>
    public void SubscribeToMessages()
    {
        _ = Subscribe();
    }

    private async Task Subscribe()
    {
        await Task.Delay(100);
        MessageCenter.PersistentSubscribe<GameLoadFinishedMessage>(OnGameLoadFinishedMessage);
        // MessageCenter.PersistentSubscribe<SOIEnteredMessage>()
        // MessageCenter.PersistentSubscribe<ResearchReportScoredMessage>(OnResearchReportScoredMessage);
        // MessageCenter.PersistentSubscribe<VesselScienceSituationChangedMessage>(OnVesselScienceSituationChangedMessage);
        MessageCenter.PersistentSubscribe<GameStateChangedMessage>(HideWindowOnInvalidState);
        // MessageCenter.PersistentSubscribe<TechTierUnlockedMessage>(OnTechTierUnlockedMessage);

        Core.Instance.EventsRegistry.SetupListeners();
    }

    public SubscriptionHandle Subscribe<TMessage>(Action<MessageCenterMessage> callback)
        where TMessage : MessageCenterMessage
    {
        return MessageCenter.PersistentSubscribe<TMessage>(callback);
    }

    private void OnGameLoadFinishedMessage(MessageCenterMessage message)
    {
        // Load Wings from WingsConfig
        Core.Instance.WingsPool.LoadRegisteredConfigs();
        // Load GameData
        SaveManager.Instance.LoadGameDataInSession();
    }

    private void OnSOIEnteredMessage(MessageCenterMessage message)
    {
    }

    // private static void OnResearchReportScoredMessage(MessageCenterMessage message)
    // {
    //     MainUIManager.Instance.ArchiveWindowController.IsDirty = true;
    // }
    //
    // private static void OnVesselScienceSituationChangedMessage(MessageCenterMessage message)
    // {
    //     // Beware, this message is sent for every vessel, not just the active one.
    //     MainUIManager.Instance.ArchiveWindowController.IsDirty = true;
    // }
    //
    // /// <summary>
    // /// Reload the available experiments when a new tech tier is unlocked.
    // /// </summary>
    // /// <param name="message"></param>
    // private static void OnTechTierUnlockedMessage(MessageCenterMessage message)
    // {
    //     ArchiveManager.Instance.InitializeUnlockedExperiments();
    //     MainUIManager.Instance.ArchiveWindowController.IsDirty = true;
    // }

    private void HideWindowOnInvalidState(MessageCenterMessage message)
    {
        if (GameStateManager.Instance.IsInvalidState())
            // Close the windows if the game is in an invalid state
            MainUIManager.Instance.AppWindow.IsWindowOpen = false;
    }
}