using BepInEx.Logging;
using KSP.Game;
using LibNoise.Modifiers;
using WayfarersWings.Extensions;
using WayfarersWings.Managers;
using WayfarersWings.Models.Configs;
using WayfarersWings.Models.Configs.Template;
using WayfarersWings.Models.Wing;

namespace WayfarersWings.Models.Wings;

public class WingsPool
{
    private static ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("WingsPool");
    public List<Wing> Wings { get; set; } = [];

    /// <summary>
    /// Maps a trigger type (event) to a list of wings that are triggered by that type.
    /// </summary>
    public Dictionary<Type, List<Wing>> TriggersMap { get; set; } = [];

    public void ImportConfig(WingsConfig wingsConfig)
    {
        foreach (var templateConfig in wingsConfig.templates)
        {
            if (templateConfig.celestialBody != null)
            {
                AddTemplateForBodies(templateConfig);
            }
            else
            {
                AddTemplateWing(templateConfig, templateConfig.template);
            }
        }

        foreach (var wingConfig in wingsConfig.wings)
        {
            AddWing(wingConfig);
        }
    }

    private void AddTemplateWing(WingTemplateConfig templateConfig, WingConfig wingConfig)
    {
        if (templateConfig.hasFirst != null)
        {
            var firstWingConfig = wingConfig.Clone();
            firstWingConfig.name = $"{wingConfig.name}_first";
            firstWingConfig.isFirst = true;
            firstWingConfig.description = templateConfig.hasFirst.description;
            firstWingConfig.imageLayers.Insert(1, templateConfig.hasFirst.imageLayer);
            AddWing(firstWingConfig);
        }

        AddWing(wingConfig);
    }

    private void AddTemplateForBodies(WingTemplateConfig templateConfig)
    {
        _logger.LogDebug($"Adding template {templateConfig.name} for celestial bodies");
        foreach (var bodyConfig in Core.Instance.PlanetsPool)
        {
            var body = GameManager.Instance.Game.UniverseModel.FindCelestialBodyByName(bodyConfig.name);
            if (body == null)
            {
                _logger.LogWarning($"Could not find celestial body '{bodyConfig.name}'");
                continue;
            }

            var wingConfig = templateConfig.template.Clone();
            wingConfig.name = $"{bodyConfig.code}_{templateConfig.name}";
            wingConfig.imageLayers.Insert(0, bodyConfig.imageLayer);
            foreach (var conditionConfig in wingConfig.conditions)
            {
                conditionConfig.Data["celestialBody"] = bodyConfig.name;
            }

            AddTemplateWing(templateConfig, wingConfig);
        }
    }

    private void AddWing(WingConfig wingConfig)
    {
        _logger.LogDebug($"Adding wing {wingConfig.name}");
        var wing = new Wing(wingConfig);
        Wings.Add(wing);

        foreach (var trigger in wingConfig.triggers)
        {
            if (!TriggersMap.ContainsKey(trigger.eventType))
            {
                TriggersMap[trigger.eventType] = [];
            }

            TriggersMap[trigger.eventType].Add(wing);
        }
    }
}