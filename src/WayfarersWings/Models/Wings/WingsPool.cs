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
    public const int PlanetPointsStart = 0;
    public const string FirstSuffix = "_first";

    private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("WayfarersWings.WingsPool");

    /// <summary>
    /// Configs to be loaded in the pool 
    /// </summary>
    private readonly List<WingsConfig> _wingsConfigs = [];

    /// <summary>
    /// List of all the wings in the pool, which are loaded on game load.
    /// </summary>
    public List<Wing> Wings { get; set; } = [];

    private readonly Dictionary<string, Wing> _wingsMap = [];

    public bool IsInitialized { get; private set; } = false;

    /// <summary>
    /// Default image layers for ranked wings.
    /// </summary>
    private static readonly List<string[]> RankedImageLayers =
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
        ["Assets/Wings/Layers/first.png", "Assets/Wings/Layers/rank6.png"],
        ["Assets/Wings/Layers/bands_superior.png", "Assets/Wings/Layers/first.png", "Assets/Wings/Layers/rank1.png"],
        ["Assets/Wings/Layers/bands_superior.png", "Assets/Wings/Layers/first.png", "Assets/Wings/Layers/rank2.png"],
        ["Assets/Wings/Layers/bands_superior.png", "Assets/Wings/Layers/first.png", "Assets/Wings/Layers/rank3.png"],
        ["Assets/Wings/Layers/bands_superior.png", "Assets/Wings/Layers/first.png", "Assets/Wings/Layers/rank4.png"],
        ["Assets/Wings/Layers/bands_superior.png", "Assets/Wings/Layers/first.png", "Assets/Wings/Layers/rank5.png"],
        ["Assets/Wings/Layers/bands_superior.png", "Assets/Wings/Layers/first.png", "Assets/Wings/Layers/rank6.png"],
    ];


    /// <summary>
    /// Maps a trigger type (event) to a list of wings that are triggered by that type.
    /// </summary>
    public Dictionary<Type, List<Wing>> TriggersMap { get; set; } = [];

    /// <summary>
    /// First step. Load all the configs in the pool, ready to be loaded
    /// </summary>
    public void RegisterConfig(WingsConfig wingsConfig)
    {
        _wingsConfigs.Add(wingsConfig);
    }

    /// <summary>
    /// Second step. Load all the registered configs in the pool
    /// </summary>
    public void LoadRegisteredConfigs()
    {
        Wings.Clear();
        _wingsMap.Clear();
        TriggersMap.Clear();

        foreach (var wingsConfig in _wingsConfigs)
        {
            LoadRegisteredConfig(wingsConfig);
        }

        IsInitialized = true;
        Logger.LogInfo($"Loaded {Wings.Count} wings");
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
        Logger.LogDebug($"Adding template {templateConfig.name} for celestial bodies");
        foreach (var bodyConfig in Core.Instance.PlanetsPool)
        {
            var body = GameManager.Instance.Game.UniverseModel.FindCelestialBodyByName(bodyConfig.name);
            if (body == null)
            {
                Logger.LogWarning($"Could not find celestial body '{bodyConfig.name}'");
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
    /// Used for templates that have a rank, so chained wings are created.
    /// </summary>
    private void AddTemplateRanked(WingTemplateConfig templateConfig)
    {
        Logger.LogInfo("Adding template " + templateConfig.name + " for ranked wings");
        for (int i = 0; i < templateConfig.ranked!.partials.Count; i++)
        {
            var partialConfig = templateConfig.ranked.partials[i];

            var wingConfig = templateConfig.template.Clone();
            // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
            wingConfig.name = partialConfig.name ?? $"{templateConfig.name}_{i + 1}";
            wingConfig.points += i;

            if (i < RankedImageLayers.Count)
            {
                foreach (var rankImageLayer in RankedImageLayers.ElementAt(i))
                {
                    wingConfig.imageLayers.Add(rankImageLayer);
                }
            }
            else
            {
                Logger.LogWarning("Not enough rank images for template " + templateConfig.name);
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
        Logger.LogDebug($"Adding wing {wingConfig.name}");
        var wing = new Wing(wingConfig);
        if (wing.config.description == null)
        {
            Logger.LogWarning("Wing " + wing.config.name + " has no description!");
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
        if (wing.config.isFirst && wing.config.hasDisplayNameFirstAlready != true)
        {
            wing.DisplayName = LocalizedStrings.GetTranslationWithParams(LocalizedStrings.FirstTimeName,
                new()
                {
                    { "name", wing.DisplayName }
                });
            wing.Description = LocalizedStrings.GetTranslationWithParams(LocalizedStrings.FirstTime,
                new()
                {
                    { "name", wing.DisplayName },
                    { "description", wing.Description }
                });
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