using System.Diagnostics;
using BepInEx.Logging;
using WayfarersWings.Models.Session;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Managers;

public class AchievementsOrchestrator
{
    private static readonly ManualLogSource Logger =
        BepInEx.Logging.Logger.CreateLogSource("WayfarersWings.AchievementsOrchestrator");

    public static AchievementsOrchestrator Instance { get; } = new();


    public static void DispatchTransaction(Transaction transaction)
    {
        if (!Core.Instance.WingsPool.IsInitialized) return;

        var triggeredWings = Core.Instance.WingsPool.Wings;
        if (transaction.Message != null)
        {
            if (!Core.Instance.WingsPool.TriggersMap.TryGetValue(transaction.Message.GetType(), out triggeredWings))
            {
                Logger.LogDebug("No wings triggered by " + transaction.Message.GetType().Name);
                triggeredWings = Core.Instance.WingsPool.Wings;
            }
        }

        var stopwatch = new Stopwatch();
        foreach (var wing in triggeredWings)
        {
            if (!wing.Check(transaction)) continue;

            Logger.LogDebug($"Triggered wing {wing.config.name} for {transaction.Message?.GetType().Name}");

            var kerbals = transaction.GetKerbals();
            WingsSessionManager.Instance.AwardAll(wing, kerbals);
        }

        stopwatch.Stop();
        Logger.LogDebug($"[batch] Triggered all wings in {stopwatch.ElapsedMilliseconds}ms");
    }
}