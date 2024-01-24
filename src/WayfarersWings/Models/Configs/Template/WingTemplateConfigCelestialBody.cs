using KSP.Sim.impl;
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

    /// <summary>
    /// Some wings are only valid for certain celestial bodies.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public bool IsValid(CelestialBodyComponent celestialBody)
    {
        foreach (var requirement in requires)
        {
            var isValid = requirement switch
            {
                CelestialBodyVariantFlag.HasSurface => celestialBody.hasSolidSurface,
                CelestialBodyVariantFlag.HasAtmosphere => celestialBody.hasAtmosphere,
                CelestialBodyVariantFlag.HasOcean => celestialBody.hasOcean,
                CelestialBodyVariantFlag.IsMoon => celestialBody.referenceBody?.IsStar == false,
                CelestialBodyVariantFlag.IsPlanet => celestialBody.referenceBody?.IsStar == true,
                CelestialBodyVariantFlag.IsStar => celestialBody.IsStar,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (!isValid) return false;
        }

        return true;
    }
}