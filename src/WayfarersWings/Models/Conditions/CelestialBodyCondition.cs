using KSP.Game;
using KSP.Sim.impl;
using WayfarersWings.Extensions;
using WayfarersWings.Models.Configs;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Conditions;

public class CelestialBodyCondition : BaseCondition
{
    public CelestialBodyComponent? CelestialBody { get; set; }

    public override bool IsValid(Transaction transaction)
    {
        return transaction.Vessel?.mainBody.Equals(CelestialBody) ?? false;
    }

    public override void Configure(ConditionConfig config)
    {
        if (config.Data.TryGetValue("celestialBody", out var bodyNameToken))
        {
            if (!bodyNameToken.TryDeserializeStringOrNull(out var bodyName))
                throw new InvalidCastException("celestialBody must be a string");

            if (!string.IsNullOrEmpty(bodyName))
                CelestialBody = GameManager.Instance.Game.UniverseModel.FindCelestialBodyByName(bodyName);
        }
    }
}