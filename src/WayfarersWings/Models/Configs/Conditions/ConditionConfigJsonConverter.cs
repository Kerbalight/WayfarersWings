using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WayfarersWings.Models.Configs.Conditions.Attributes;

namespace WayfarersWings.Models.Configs.Conditions;

public class ConditionConfigJsonConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    private static readonly Dictionary<string, Type> ConditionTypes = new();

    static ConditionConfigJsonConverter()
    {
        var attributes = typeof(ConditionConfig).GetCustomAttributes(typeof(ConditionConfigTypeAttribute),
            inherit: false);

        foreach (var attribute in attributes)
        {
            if (attribute is ConditionConfigTypeAttribute configTypeAttribute)
            {
                ConditionTypes[configTypeAttribute.Discriminator] = configTypeAttribute.ConfigType;
            }
        }
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        var obj = JObject.Load(reader);
        var type = obj["type"]?.Value<string>();
        if (type == null)
        {
            throw new InvalidOperationException("Condition type not specified");
        }

        var conditionType = ConditionTypes[type];
        return null;
    }

    public override bool CanConvert(Type objectType)
    {
        return typeof(ConditionConfig).IsAssignableFrom(objectType);
    }
}