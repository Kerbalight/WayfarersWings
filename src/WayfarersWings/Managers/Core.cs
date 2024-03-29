﻿using System.Reflection;
using BepInEx.Logging;
using KSP.Game;
using KSP.Messages;
using KSP.Sim.impl;
using UnityEngine;
using WayfarersWings.Managers.Observer;
using WayfarersWings.Models.Conditions.Events;
using WayfarersWings.Models.Configs;
using WayfarersWings.Models.Configs.Planets;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Managers;

public class Core
{
    private static ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("Core");

    public static Core Instance { get; } = new();

    public static VesselComponent? ActiveVessel => GameManager.Instance.Game.ViewController.GetActiveSimVessel();

    public static UniverseModel? UniverseModel => GameManager.Instance?.Game?.UniverseModel;

    public static MessageCenter Messages => GameManager.Instance.Game.Messages;

    public string SessionGuidString { get; set; } = null!;

    public ConditionEventsRegistry EventsRegistry { get; } = new();
    public KerbalStateObserver KerbalStateObserver { get; } = new();

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
        WingsConfig? wingsConfig;
        try
        {
            wingsConfig = WingsConfig.Deserialize(configAsset.text, out var exceptions);

            foreach (var exception in exceptions)
            {
                _logger.LogError("Exception while deserializing wings config:");
                _logger.LogError(exception);
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed to deserialize wings config {configAsset.name}: {e}");
            _logger.LogError(e);
            return;
        }

        if (wingsConfig == null)
        {
            _logger.LogError($"Failed to import wings from {configAsset.name}");
            return;
        }

        WingsPool.RegisterConfig(wingsConfig);
    }

    public static double GetUniverseTime()
    {
        return UniverseModel!.UniverseTime;
    }
}