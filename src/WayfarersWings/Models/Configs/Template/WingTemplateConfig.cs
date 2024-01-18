namespace WayfarersWings.Models.Configs.Template;

[Serializable]
public class WingTemplateConfig
{
    public string name;
    public WingTemplateConfigCelestialBody? celestialBody;
    public WingTemplateConfigHasFirst? hasFirst;
    public WingConfig template;
}