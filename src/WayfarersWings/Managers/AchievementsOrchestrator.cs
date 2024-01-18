using WayfarersWings.Models.Wings;

namespace WayfarersWings.Managers;

public class AchievementsOrchestrator
{
    public static AchievementsOrchestrator Instance { get; } = new();

    public void DispatchTransaction(Transaction transaction)
    {
        var triggeredWings = transaction.Message != null
            ? Core.Instance.WingsPool.TriggersMap[transaction.Message.GetType()]
            : Core.Instance.WingsPool.Wings;

        foreach (var wing in triggeredWings)
        {
            wing.Check(transaction);
        }
    }
}