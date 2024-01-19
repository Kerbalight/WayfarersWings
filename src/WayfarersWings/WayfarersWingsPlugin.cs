using System.Reflection;
using BepInEx;
using JetBrains.Annotations;
using SpaceWarp;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Loading;
using SpaceWarp.API.Mods;
using SpaceWarp.API.UI.Appbar;
using WayfarersWings.UI;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;
using WayfarersWings.Extensions;
using WayfarersWings.Managers;
using WayfarersWings.Utility;

namespace WayfarersWings;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class WayfarersWingsPlugin : BaseSpaceWarpPlugin
{
    // Useful in case some other mod wants to use this mod a dependency
    [PublicAPI] public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    [PublicAPI] public const string ModName = MyPluginInfo.PLUGIN_NAME;
    [PublicAPI] public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    /// Singleton instance of the plugin class
    [PublicAPI]
    public static WayfarersWingsPlugin Instance { get; set; }

    // AppBar button IDs
    internal const string ToolbarFlightButtonID = "BTN-WayfarersWingsFlight";
    internal const string ToolbarOabButtonID = "BTN-WayfarersWingsOAB";
    internal const string ToolbarKscButtonID = "BTN-WayfarersWingsKSC";

    private void Awake()
    {
        Logger.LogDebug($"Registering 'ImportAddressableWingsPlanets' as a loading action.");
        Loading.AddAddressablesLoadingAction<TextAsset>(
            "Loading Wings planets configurations",
            Constants.WingsPlanetsAddressableLabel,
            Core.Instance.ImportPlanetsConfig
        );

        Logger.LogDebug($"Registering 'ImportAddressableWingsData' as a loading action.");
        Loading.AddAddressablesLoadingAction<TextAsset>(
            "Loading Wings data configurations",
            Constants.WingsDataAddressableLabel,
            Core.Instance.ImportWingsConfig
        );

        Logger.LogDebug($"Registering 'ImportVisualTreeAsset' as a loading action.");
        SpaceWarpExtensions.AddUIAddressablesLoadingAction<VisualTreeAsset>(
            "Loading Wings UI",
            Constants.WingsUIAddressableLabel,
            MainUIManager.Instance.ImportVisualTreeAsset
        );
    }

    /// <summary>
    /// Runs when the mod is first initialized.
    /// </summary>
    public override void OnInitialized()
    {
        base.OnInitialized();

        Instance = this;

        // Load all the other assemblies used by this mod
        LoadAssemblies();

        // Load UI
        MainUIManager.Instance.Initialize();

        // Register Flight AppBar button
        Appbar.RegisterAppButton(
            ModName,
            ToolbarFlightButtonID,
            AssetManager.GetAsset<Texture2D>($"{ModGuid}/images/icon.png"),
            isOpen => MainUIManager.Instance.AppWindow.IsWindowOpen = isOpen
        );

        // Register OAB AppBar Button
        Appbar.RegisterOABAppButton(
            ModName,
            ToolbarOabButtonID,
            AssetManager.GetAsset<Texture2D>($"{ModGuid}/images/icon.png"),
            isOpen => MainUIManager.Instance.AppWindow.IsWindowOpen = isOpen
        );

        // Register KSC AppBar Button
        Appbar.RegisterKSCAppButton(
            ModName,
            ToolbarKscButtonID,
            AssetManager.GetAsset<Texture2D>($"{ModGuid}/images/icon.png"),
            () => MainUIManager.Instance.AppWindow.IsWindowOpen = !MainUIManager.Instance.AppWindow.IsWindowOpen
        );

        // Messages subscribe
        MessageListener.Instance.SubscribeToMessages();
    }

    /// <summary>
    /// Loads all the assemblies for the mod.
    /// </summary>
    private static void LoadAssemblies()
    {
        // Load the Unity project assembly
        var currentFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory!.FullName;
        var unityAssembly = Assembly.LoadFrom(Path.Combine(currentFolder, "WayfarersWings.Unity.dll"));
        // Register any custom UI controls from the loaded assembly
        CustomControls.RegisterFromAssembly(unityAssembly);
    }
}