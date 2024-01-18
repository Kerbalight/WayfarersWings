using WayfarersWings.Models.Conditions;
using WayfarersWings.Models.Configs;

namespace WayfarersWings.Models.Wings;

public class Wing
{
    public WingConfig Config { get; set; }

    private readonly List<BaseCondition> _conditions = [];

    public Wing(WingConfig config)
    {
        Config = config;
        foreach (var conditionConfig in Config.conditions)
        {
            var conditionType = Type.GetType($"WayfarersWings.Models.Conditions.{conditionConfig.type}");
            if (conditionType == null)
            {
                throw new InvalidOperationException($"Could not find condition type '{conditionConfig.type}'");
            }

            var conditionInstance = Activator.CreateInstance(conditionType) as BaseCondition;
            conditionInstance!.Configure(conditionConfig);
            _conditions.Add(conditionInstance);
        }
    }

    public void Check(Transaction transaction)
    {
        foreach (var condition in _conditions)
        {
            if (!condition.IsValid(transaction)) return;
        }
        // TODO: Add wing to vessel
    }
}