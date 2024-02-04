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

        var messageName = transaction.Message?.GetType().Name ?? "null";

        var triggeredWings = Core.Instance.WingsPool.Wings;
        if (transaction.Message != null)
        {
            if (!Core.Instance.WingsPool.TriggersMap.TryGetValue(transaction.Message.GetType(), out triggeredWings))
            {
                // Logger.LogDebug($"No wings triggered by {messageName}");
                triggeredWings = Core.Instance.WingsPool.Wings;
            }
        }

        var stopwatch = new Stopwatch();
        var validWingsCount = 0;
        foreach (var wing in triggeredWings)
        {
            if (!wing.Check(transaction)) continue;

            validWingsCount++;
            Logger.LogDebug($"Valid wing {wing.config.name} for {messageName}");

            var kerbals = transaction.GetKerbals();
            WingsSessionManager.Instance.AwardAll(wing, kerbals);
        }

        stopwatch.Stop();
        if (validWingsCount > 0)
            Logger.LogDebug(
                $"[batch] {validWingsCount} valid wings in {stopwatch.ElapsedMilliseconds}ms for {messageName}");
    }
}