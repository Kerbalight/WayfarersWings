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

    public const int PlanetPointsStart = 0;

    private static List<string[]> _rankedImageLayers =
    [
        [],
        ["Assets/Wings/Layers/rank1.png"],
        ["Assets/Wings/Layers/rank2.png"],
        ["Assets/Wings/Layers/rank3.png"],
        ["Assets/Wings/Layers/rank4.png"],
        ["Assets/Wings/Layers/rank5.png"],
        ["Assets/Wings/Layers/rank6.png"],
        ["Assets/Wings/Layers/first.png", "Assets/Wings/Layers/rank1.png"],
        ["Assets/Wings/Layers/first.png", "Assets/Wings/Layers/rank2.png"],
        ["Assets/Wings/Layers/first.png", "Assets/Wings/Layers/rank3.png"],
        ["Assets/Wings/Layers/first.png", "Assets/Wings/Layers/rank4.png"],
        ["Assets/Wings/Layers/first.png", "Assets/Wings/Layers/rank5.png"],
        ["Assets/Wings/Layers/first.png", "Assets/Wings/Layers/rank6.png"]
    ];

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
            else if (templateConfig.ranked != null)
            {
                AddTemplateRanked(templateConfig);
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
            firstWingConfig.points += 1;
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

            if (!templateConfig.celestialBody!.IsValid(body)) continue;

            var wingConfig = templateConfig.template.Clone();
            wingConfig.name = $"{bodyConfig.code}_{templateConfig.name}";
            wingConfig.imageLayers.Insert(0, bodyConfig.imageLayer);
            wingConfig.points += PlanetPointsStart + bodyConfig.points;
            foreach (var condition in wingConfig.conditions)
            {
                if (condition is CelestialBodyCondition { celestialBody: null } celestialBodyCondition)
                {
                    celestialBodyCondition.celestialBody = bodyConfig.name;
                }
            }

            wingConfig.localizationParams.Add("body", body.DisplayName);


            AddTemplateWing(templateConfig, wingConfig);
        }
    }

    /// <summary>
    /// Should be working, not tested.
    /// </summary>
    /// <param name="templateConfig"></param>
    private void AddTemplateRanked(WingTemplateConfig templateConfig)
    {
        _logger.LogInfo("Adding template " + templateConfig.name + " for ranked wings");
        for (int i = 0; i < templateConfig.ranked!.partials.Count; i++)
        {
            var partialConfig = templateConfig.ranked.partials[i];

            var wingConfig = templateConfig.template.Clone();
            // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
            wingConfig.name = partialConfig.name ?? $"{templateConfig.name}_{i + 1}";
            wingConfig.points += i;

            if (i < _rankedImageLayers.Count)
            {
                foreach (var rankImageLayer in _rankedImageLayers.ElementAt(i))
                {
                    wingConfig.imageLayers.Add(rankImageLayer);
                }
            }
            else
            {
                _logger.LogWarning("Not enough rank images for template " + templateConfig.name);
            }

            if (partialConfig.description != null)
                wingConfig.description = partialConfig.description;

            wingConfig.conditions.AddRange(partialConfig.conditions);
            wingConfig.localizationParams.AddRange(partialConfig.localizationParams);
            wingConfig.localizationParams.Add("rank", RomanNumbers.Convert(i + 1));

            AddTemplateWing(templateConfig, wingConfig);
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

        wing.DisplayName =
            LocalizedStrings.GetTranslationWithParams(wing.config.displayName, wing.config.localizationParams);
        wing.Description =
            LocalizedStrings.GetTranslationWithParams(wing.config.description, wing.config.localizationParams);
        if (wing.config.isFirst)
        {
            wing.DisplayName += " " + "1°";
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