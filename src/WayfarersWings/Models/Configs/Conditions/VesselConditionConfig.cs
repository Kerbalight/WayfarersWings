using KSP.Sim.impl;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WayfarersWings.Models.Configs.Conditions;

[Serializable]
public class VesselConditionConfig : ConditionConfig
{
    [JsonConverter(typeof(StringEnumConverter))]
    public VesselSituations? situation;
}