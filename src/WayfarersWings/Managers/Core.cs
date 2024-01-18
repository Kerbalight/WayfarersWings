using BepInEx.Logging;
using UnityEngine;
using WayfarersWings.Models.Configs;
using WayfarersWings.Models.Configs.Planets;
using WayfarersWings.Models.Wing;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Managers;

public class Core
{
    private static ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("Core");

    public static Core Instance { get; } = new();

    public WingsPool WingsPool { get; set; } = new();
    public List<CelestialBodyConfig> PlanetsPool { get; set; } = [];

    public void ImportPlanetsConfig(TextAsset configAsset)
    {
        _logger.LogInfo($"Importing planets from {configAsset.name} ({configAsset.dataSize} bytes)");
        var planetsConfig = PlanetsConfig.Deserialize(configAsset.text);
        if (planetsConfig == null)
        {
            _logger.LogError($"Failed to import planets from {configAsset.name}");
            return;
        }

        PlanetsPool.AddRange(planetsConfig.celestialBodies);
    }

    public void ImportWingsConfig(TextAsset configAsset)
    {
        _logger.LogInfo($"Importing wings from {configAsset.name} ({configAsset.dataSize} bytes)");
        var wingsConfig = WingsConfig.Deserialize(configAsset.text);
        if (wingsConfig == null)
        {
            _logger.LogError($"Failed to import wings from {configAsset.name}");
            return;
        }

        WingsPool.ImportConfig(wingsConfig);
    }
}