using KSP.Sim.impl;
using Newtonsoft.Json;
using WayfarersWings.Models.Configs;
using WayfarersWings.Models.Configs.Conditions;
using WayfarersWings.Models.Configs.Conditions.Attributes;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Conditions;

[Serializable]
[ConditionType("Vessel", typeof(VesselCondition))]
[ConditionType("Orbit", typeof(OrbitCondition))]
[ConditionType("Eva", typeof(EvaCondition))]
[ConditionType("SphereOfInfluence", typeof(SphereOfInfluenceCondition))]
public abstract class BaseCondition
{
    public string type;

    public void Register()
    {
    }

    public abstract bool IsValid(Transaction transaction);

    public virtual void Configure()
    {
    }
}