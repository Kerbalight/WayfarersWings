using WayfarersWings.Models.Conditions.Json;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Conditions;

[Serializable]
[ConditionType("Vessel", typeof(VesselCondition))]
[ConditionType("Orbit", typeof(OrbitCondition))]
[ConditionType("Kerbal", typeof(KerbalCondition))]
[ConditionType("Travel", typeof(TravelCondition))]
[ConditionType("FlagPlanted", typeof(FlagPlantedCondition))]
[ConditionType("SphereOfInfluence", typeof(SphereOfInfluenceCondition))]
public abstract class BaseCondition
{
    public string type;

    public void Register() { }

    public abstract bool IsValid(Transaction transaction);

    public virtual void Configure() { }
}