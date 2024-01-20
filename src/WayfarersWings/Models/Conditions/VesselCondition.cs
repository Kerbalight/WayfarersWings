using KSP.Sim.impl;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Conditions;

[Serializable]
public class VesselCondition : BaseCondition
{
    [JsonConverter(typeof(StringEnumConverter))]
    public VesselSituations? situation;

    public override bool IsValid(Transaction transaction)
    {
        if (situation != null && transaction.Vessel?.Situation != situation)
            return false;

        return true;
    }

    public override void Configure()
    {
    }
}