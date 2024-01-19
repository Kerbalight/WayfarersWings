using BepInEx.Logging;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Managers;

public class AchievementsOrchestrator
{
    private static readonly ManualLogSource Logger =
        BepInEx.Logging.Logger.CreateLogSource("WayfarersWings.AchievementsOrchestrator");

    public static AchievementsOrchestrator Instance { get; } = new();


    public void DispatchTransaction(Transaction transaction)
    {
        var triggeredWings = Core.Instance.WingsPool.Wings;
        if (transaction.Message != null)
        {
            if (!Core.Instance.WingsPool.TriggersMap.TryGetValue(transaction.Message.GetType(), out triggeredWings))
            {
                Logger.LogDebug("No wings triggered by " + transaction.Message.GetType().Name);
            }
        }

        foreach (var wing in triggeredWings)
        {
            if (!wing.Check(transaction)) continue;

            Logger.LogDebug("Triggered wing " + wing.Config.name);
            var kerbals = transaction.GetKerbals();
            foreach (var kerbal in kerbals)
            {
                Logger.LogDebug(" -> Will award " + wing.Config.name + " to kerbal " + kerbal.Attributes.GetFullName());
                // wing.Trigger(kerbal);
            }
        }
    }
}