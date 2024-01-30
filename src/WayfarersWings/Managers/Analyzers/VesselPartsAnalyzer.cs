using BepInEx.Logging;
using KSP.Game;
using KSP.Modules;
using KSP.Sim.DeltaV;
using KSP.Sim.impl;
using KSP.Sim.ResourceSystem;

namespace WayfarersWings.Managers.Analyzers;

public static class VesselPartsAnalyzer
{
    private static readonly ManualLogSource Logger =
        BepInEx.Logging.Logger.CreateLogSource("WayfarersWings.VesselPartsAnalyzer");

    public static ResourceDefinitionID SolidFuelDefinitionId =>
        GameManager.Instance.Game.ResourceDefinitionDatabase.GetResourceIDFromName("SolidFuel");

    // public IEnumerable<Data_Engine> GetCurrentStageEngines(VesselComponent? vessel)
    // {
    //     if (vessel == null) return Enumerable.Empty<Data_Engine>();
    //     var parts = vessel.SimulationObject.PartOwner.Parts;
    //     var currentStage = vessel.SimulationObject.Staging.LastStage;
    //     var engines = new List<Data_Engine>();
    //     foreach (var part in parts)
    //     {
    //         if (part.IsPartEngine(out var dataEngine) && !dataEngine.currentEngineModeData.nonThrustMotor)
    //             engines.Add(dataEngine);
    //     }
    //
    //     return engines;
    // }


    public static double GetSolidFuelPercentage(VesselComponent? vessel)
    {
        if (vessel == null) return 0;

        var totalMass = vessel.totalMass;

        var parts = vessel.SimulationObject.PartOwner.Parts;
        var totalSolidFuel = 0d;
        foreach (var part in parts)
        {
            if (!part.IsPartEngine(out var dataEngine)) continue;

            // We want to ignore non-thrust motors and engines that are not solid boosters
            if (dataEngine.currentEngineModeData.nonThrustMotor ||
                dataEngine.currentEngineModeData.engineType != EngineType.SolidBooster) continue;

            foreach (var resourceContainer in dataEngine.CurrentPropellantState.PartResourceContainer)
            {
                totalSolidFuel += resourceContainer.GetResourceStoredMass(SolidFuelDefinitionId);
            }
        }

        var solidFuelPercentage = totalSolidFuel / totalMass;
        Logger.LogDebug(
            $"Solid fuel percentage: {solidFuelPercentage} (fuel={totalSolidFuel},mass={totalMass}) (vessel={vessel.Name}");
        return solidFuelPercentage;
    }

    public static VesselParachutesState GetParachutesState(VesselComponent vessel)
    {
        var parts = vessel.SimulationObject.PartOwner.Parts;
        var state = new VesselParachutesState();
        foreach (var part in parts)
        {
            if (!part.TryGetModuleData<PartComponentModule_Parachute, Data_Parachute>(out var dataParachute)) continue;

            switch (dataParachute.deployState.GetValue())
            {
                case Data_Parachute.DeploymentStates.DEPLOYED:
                    state.deployedCount++;
                    break;
                case Data_Parachute.DeploymentStates.CUT:
                    state.cutCount++;
                    break;
                case Data_Parachute.DeploymentStates.SEMIDEPLOYED:
                    state.semiDeployedCount++;
                    break;
                case Data_Parachute.DeploymentStates.STOWED:
                    state.stowedCount++;
                    break;
                case Data_Parachute.DeploymentStates.ARMED:
                    state.activeCount++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return state;
    }

    public static List<Data_WheelBase> GetRoverWheels(VesselComponent vessel)
    {
        var parts = vessel.SimulationObject.PartOwner.Parts;
        List<Data_WheelBase> wheels = [];

        foreach (var part in parts)
        {
            if (!part.TryGetModuleData<PartComponentModule_WheelBase, Data_WheelBase>(out var dataWheel)) continue;
            // I'd like to check for `IsGrounded` but here we can't do that,
            // since this is called just OnStart, and the vessel may not yet be
            // on the ground.
            if (dataWheel.WheelType == WheelType.MOTORIZED) wheels.Add(dataWheel);
        }

        return wheels;
    }
}