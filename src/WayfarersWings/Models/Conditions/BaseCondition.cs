using KSP.Sim.impl;
using Newtonsoft.Json;
using WayfarersWings.Models.Configs;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Conditions;

public abstract class BaseCondition
{
    public void Register()
    {
    }

    public abstract bool IsValid(Transaction transaction);

    public virtual void Configure(ConditionConfig config)
    {
    }
}