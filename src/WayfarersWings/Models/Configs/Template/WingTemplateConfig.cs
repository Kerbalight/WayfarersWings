using Newtonsoft.Json;

namespace WayfarersWings.Models.Configs.Template;

[Serializable]
public class WingTemplateConfig
{
    [JsonRequired]
    public string name = null!;

    public WingTemplateConfigCelestialBody? celestialBody;
    public WingTemplateConfigHasFirst? hasFirst;
    public WingTemplateConfigRanked? ranked;

    [JsonRequired]
    public WingConfig template = null!;
}