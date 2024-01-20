using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WayfarersWings.Models.Conditions.Json;

public class BaseConditionJsonConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    private static readonly Dictionary<string, Type> ConditionTypes = new();

    static BaseConditionJsonConverter()
    {
        var attributes = typeof(BaseCondition).GetCustomAttributes(typeof(ConditionTypeAttribute),
            inherit: false);

        foreach (var attribute in attributes)
        {
            if (attribute is ConditionTypeAttribute conditionTypeAttribute)
            {
                ConditionTypes[conditionTypeAttribute.Discriminator] = conditionTypeAttribute.ConditionType;
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
        var condition = Activator.CreateInstance(conditionType);
        serializer.Populate(obj.CreateReader(), condition);
        return condition;
    }

    public override bool CanConvert(Type objectType)
    {
        return typeof(BaseCondition).IsAssignableFrom(objectType);
    }

    public override bool CanWrite => false;
}