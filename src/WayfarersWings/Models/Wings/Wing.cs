using WayfarersWings.Models.Conditions;
using WayfarersWings.Models.Configs;

namespace WayfarersWings.Models.Wings;

public class Wing
{
    // ReSharper disable once InconsistentNaming
    public WingConfig config { get; set; }

    public string DisplayName { get; set; }
    public string Description { get; set; }

    private readonly List<BaseCondition> _conditions = [];

    public Wing(WingConfig config)
    {
        this.config = config;
        VerifyConfig(); // Todo find a better place to do this

        foreach (var condition in this.config.conditions)
        {
            condition.Configure();
            _conditions.Add(condition);
        }
    }

    /// <summary>
    /// Ensure that the wing config is valid
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private void VerifyConfig()
    {
        if (config.conditions.Count == 0)
        {
            WayfarersWingsPlugin.Instance.SWLogger.LogError($"Wing '{config.name}' has no conditions");
        }

        if (config.isFirst && !config.name.EndsWith("_first"))
            throw new InvalidOperationException(
                $"Wing '{config.name}' is marked as first but does not end with '_first'");
    }

    public bool Check(Transaction transaction)
    {
        foreach (var condition in _conditions)
        {
            if (!condition.IsValid(transaction)) return false;
        }

        return true;
        // TODO: Add wing to vessel
    }
}