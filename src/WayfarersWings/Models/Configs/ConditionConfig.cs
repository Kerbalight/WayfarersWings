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
}