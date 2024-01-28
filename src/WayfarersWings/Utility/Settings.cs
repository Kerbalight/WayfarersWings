using BepInEx.Configuration;
using WayfarersWings.UI;

namespace WayfarersWings.Utility;

public static class Settings
{
    private static WayfarersWingsPlugin Plugin => WayfarersWingsPlugin.Instance;

    // UI
    public static ConfigEntry<bool> ShowAlwaysBigRibbons { get; private set; } = null!;
    public static ConfigEntry<bool> ShowFlightReportSummary { get; private set; } = null!;

    public static void SetupConfig()
    {
        // UI Ribbons
        ShowAlwaysBigRibbons = Plugin.Config.Bind(
            "UI",
            "Show always big ribbons",
            false,
            "If true, ribbons will always be shown at full size."
        );
        ShowAlwaysBigRibbons.SettingChanged += OnSettingChanged;

        // Show flight report summary
        ShowFlightReportSummary = Plugin.Config.Bind(
            "UI",
            "Show flight report summary",
            true,
            "If true, when a vessel is recovered a mission summary window with all achieved wings will be shown."
        );
    }

    /// <summary>
    /// Refresh the UI when a setting is changed.
    /// </summary>
    private static void OnSettingChanged(object sender, EventArgs e)
    {
        MainUIManager.Instance.AppWindow.Refresh();
    }
}