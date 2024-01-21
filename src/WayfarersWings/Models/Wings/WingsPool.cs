using BepInEx.Logging;
using I2.Loc;
using KSP.Game;
using MonoMod.Utils;
using WayfarersWings.Extensions;
using WayfarersWings.Managers;
using WayfarersWings.Models.Conditions;
using WayfarersWings.Models.Configs;
using WayfarersWings.Models.Configs.Template;
using WayfarersWings.UI.Localization;

namespace WayfarersWings.Models.Wings;

public class WingsPool
{
    private static ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("WayfarersWings.WingsPool");
    public List<Wing> Wings { get; set; } = [];

    private Dictionary<string, Wing> _wingsMap = [];

    private List<WingsConfig> _wingsConfigs = [];

    public const string FirstSuffix = "_first";

    /// <summary>
    /// Maps a trigger type (event) to a list of wings that are triggered by that type.
    /// </summary>
    public Dictionary<Type, List<Wing>> TriggersMap { get; set; } = [];

    public void RegisterConfig(WingsConfig wingsConfig)
    {
        _wingsConfigs.Add(wingsConfig);
    }

    public void LoadRegisteredConfigs()
    {
        foreach (var wingsConfig in _wingsConfigs)
        {
            LoadRegisteredConfig(wingsConfig);
        }
    }

    private void LoadRegisteredConfig(WingsConfig wingsConfig)
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

    private void AddTemplateWing(WingTemplateConfig templateConfig, WingConfig wingConfig,
        Dictionary<string, string>? localizationParams = null)
    {
        if (templateConfig.hasFirst != null)
        {
            var firstWingConfig = wingConfig.Clone();
            firstWingConfig.name = $"{wingConfig.name}{FirstSuffix}";
            firstWingConfig.isFirst = true;
            // firstWingConfig.description = templateConfig.hasFirst.description;
            firstWingConfig.imageLayers.Insert(1, templateConfig.hasFirst.imageLayer);
            AddWing(firstWingConfig, localizationParams);
        }

        AddWing(wingConfig, localizationParams);
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

            var localizationParams = new Dictionary<string, string>
            {
                ["body"] = body.DisplayName
            };

            var wingConfig = templateConfig.template.Clone();
            wingConfig.name = $"{bodyConfig.code}_{templateConfig.name}";
            wingConfig.imageLayers.Insert(0, bodyConfig.imageLayer);
            foreach (var condition in wingConfig.conditions)
            {
                if (condition is CelestialBodyCondition { celestialBody: null } celestialBodyCondition)
                {
                    celestialBodyCondition.celestialBody = bodyConfig.name;
                }
            }

            AddTemplateWing(templateConfig, wingConfig, localizationParams);
        }
    }

    private void AddWing(WingConfig wingConfig, Dictionary<string, string>? localizationParams = null)
    {
        _logger.LogDebug($"Adding wing {wingConfig.name}");
        var wing = new Wing(wingConfig);
        if (wing.config.description == null)
        {
            _logger.LogWarning("Wing " + wing.config.name + " has no description!");
            wing.config.description = wing.config.name;
        }

        // Merge localization params
        if (localizationParams != null)
        {
            wing.config.localizationParams.AddRange(localizationParams);
        }

        wing.Description =
            LocalizedStrings.GetTranslationWithParams(wing.config.description, wing.config.localizationParams);
        if (wing.config.isFirst)
        {
            // TODO Configurable position?
            wing.Description += " " + LocalizedStrings.FirstTime;
        }

        Wings.Add(wing);
        _wingsMap[wingConfig.name] = wing;

        foreach (var trigger in wingConfig.triggers)
        {
            if (!TriggersMap.ContainsKey(trigger.eventType))
            {
                TriggersMap[trigger.eventType] = [];
            }

            TriggersMap[trigger.eventType].Add(wing);
        }

        foreach (var trigger in wingConfig.GetConditionsTriggerTypes())
        {
            if (!TriggersMap.ContainsKey(trigger))
            {
                TriggersMap[trigger] = [];
            }

            TriggersMap[trigger].Add(wing);
        }
    }

    public static string GetNotFirstWingName(Wing wing)
    {
        return !wing.config.isFirst
            ? string.Empty
            : wing.config.name.Remove(wing.config.name.Length - FirstSuffix.Length);
    }

    public bool TryGetWingByCode(string code, out Wing wing)
    {
        return _wingsMap.TryGetValue(code, out wing);
    }
}