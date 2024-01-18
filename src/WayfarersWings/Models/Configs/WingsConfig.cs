using Newtonsoft.Json;
using UnityEngine.Serialization;
using WayfarersWings.Models.Configs;
using WayfarersWings.Models.Configs.Template;

namespace WayfarersWings.Models.Wing;

/// <summary>
/// Allows for the configuration of the wings dynamically, through JSON config
/// files in addressables.
/// </summary>
[Serializable]
public class WingsConfig
{
    public List<WingConfig> wings = [];
    public List<WingTemplateConfig> templates = [];

    public static WingsConfig? Deserialize(string rawJson)
    {
        return JsonConvert.DeserializeObject<WingsConfig>(rawJson);
    }
}