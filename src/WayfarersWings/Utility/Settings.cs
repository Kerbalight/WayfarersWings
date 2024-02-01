using System.ComponentModel;
using BepInEx.Configuration;
using WayfarersWings.UI;

namespace WayfarersWings.Utility;

public static class Settings
{
    public enum HiDPIDisplayMode
    {
        [Description("Normal (1x)")]
        Normal,

        [Description("HiDPI (2x)")]
        Retina,

        [Description("Detect automatically")]
        Auto
    }

    private static WayfarersWingsPlugin Plugin => WayfarersWingsPlugin.Instance;

    // UI
    public static ConfigEntry<bool> ShowAlwaysBigRibbons { get; private set; } = null!;
    public static ConfigEntry<HiDPIDisplayMode> RibbonHiDPIDisplayMode { get; private set; } = null!;
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


        // HiDPI Ribbons
        RibbonHiDPIDisplayMode = Plugin.Config.Bind(
            "UI",
            "HiDPI ribbons display mode",
            HiDPIDisplayMode.Auto,
            "How to display the ribbons in the list. \n" +
            "Normal (1x): show ribbons at normal size. \n" +
            "HiDPI (2x): show ribbons at double size. \n" +
            "Detect automatically: show ribbons at double size if the screen is HiDPI."
        );

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