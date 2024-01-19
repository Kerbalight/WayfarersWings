using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WayfarersWings.Models.Conditions.Events;

namespace WayfarersWings.Models.Configs;

[Serializable]
public class ConditionConfig
{
    public string type;

    [JsonExtensionData] public Dictionary<string, JToken> Data = new();

    public T GetDatum<T>(string key)
    {
        if (!Data.TryGetValue(key, out var token) || token == null || token.Type == JTokenType.Null)
        {
            throw new Exception($"Condition of type '{type}' is missing required key '{key}'");
        }

        return token.ToObject<T>() ??
               throw new Exception($"Condition of type '{type}' has invalid value for key '{key}'");
    }

    public bool TryGetDatum<T>(string key, out T? value)
    {
        if (!Data.TryGetValue(key, out var token) || token == null || token.Type == JTokenType.Null)
        {
            value = default;
            return false;
        }

        value = token.ToObject<T?>() ??
                throw new Exception($"Condition of type '{type}' has invalid value for key '{key}'");
        return true;
    }
}