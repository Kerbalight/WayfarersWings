using Newtonsoft.Json;

namespace WayfarersWings.Models.Configs.Planets;

[Serializable]
public class PlanetsConfig
{
    public List<CelestialBodyConfig> celestialBodies = [];

    public static PlanetsConfig? Deserialize(string rawJson)
    {
        return JsonConvert.DeserializeObject<PlanetsConfig>(rawJson);
    }
}