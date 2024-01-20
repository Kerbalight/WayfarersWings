using KSP.Game;
using KSP.Messages;
using KSP.Sim.impl;
using Newtonsoft.Json;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Conditions;

[Serializable]
public class CelestialBodyCondition : BaseCondition
{
    public string? celestialBody;

    [JsonIgnore] public CelestialBodyComponent? CelestialBody { get; set; }

    public override bool IsValid(Transaction transaction)
    {
        return transaction.Vessel?.mainBody.Equals(CelestialBody) ?? false;
    }

    public override void Configure()
    {
        if (string.IsNullOrEmpty(celestialBody))
            throw new InvalidCastException("celestialBody must be a string");

        CelestialBody = GameManager.Instance.Game.UniverseModel.FindCelestialBodyByName(celestialBody);
    }
}