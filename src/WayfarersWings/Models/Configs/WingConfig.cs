using WayfarersWings.Models.Conditions;
using WayfarersWings.Models.Conditions.Events;

namespace WayfarersWings.Models.Configs;

public enum WingConfigVariant
{
    /// Multiplies this wing config for each celestial body.
    CelestialBody,
}

/// <summary>
/// Configuration for a single wing.
/// </summary>
[Serializable]
public class WingConfig
{
    /// A unique identifier for the wing.
    public string name = null!;

    public bool isFirst = false;

    /// The name of the wing as it will be displayed in the UI. This will be translated.
    public string displayName = "";

    /// A description of the wing as it will be displayed in the UI. This will be translated.
    public string? description;

    public Dictionary<string, string> localizationParams = new();

    /// The name of the wing's textures in the addressable bundle.
    /// Each layer is a separate texture which will be overlaid on top of each other.
    public List<string> imageLayers = [];

    /// A points value for the wing. This is used to indicate the importance of the wing.
    public int points;

    /// Trigger names
    public List<TriggerEvent> triggers = [];

    public List<BaseCondition> conditions = [];

    public IEnumerable<Type> GetConditionsTriggerTypes()
    {
        var types = new List<Type>();
        foreach (var condition in conditions)
        {
            var attributes = condition.GetType().GetCustomAttributes(typeof(ConditionTriggerEventAttribute), true);
            foreach (var attribute in attributes)
            {
                types.Add(((ConditionTriggerEventAttribute)attribute).TriggerEventType);
            }
        }

        return types;
    }
}