using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WayfarersWings.Models.Configs.Template;

[Serializable]
[JsonConverter(typeof(StringEnumConverter))]
public enum CelestialBodyVariantFlag
{
    HasAtmosphere,
    HasOcean,
    HasSurface,
    IsMoon,
    IsPlanet,
    IsStar,
}

[Serializable]
public class WingTemplateConfigCelestialBody
{
    public List<CelestialBodyVariantFlag> requires = [];
}