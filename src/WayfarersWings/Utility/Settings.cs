using BepInEx.Configuration;
using WayfarersWings.UI;

namespace WayfarersWings.Utility;

public static class Settings
{
    private static WayfarersWingsPlugin Plugin => WayfarersWingsPlugin.Instance;

    // Spoilers
    public static ConfigEntry<bool> ShowAlwaysBigRibbons { get; private set; } = null!;

    public static void SetupConfig()
    {
        // UI Ribbons
        ShowAlwaysBigRibbons = Plugin.Config.Bind(
            "UI",
            "Show always big ribbons",
            true,
            "If true, ribbons will always be shown at full size."
        );
        ShowAlwaysBigRibbons.SettingChanged += OnSettingChanged;
    }

    /// <summary>
    /// Refresh the UI when a setting is changed.
    /// </summary>
    private static void OnSettingChanged(object sender, EventArgs e)
    {
        MainUIManager.Instance.AppWindow.Refresh();
    }
}