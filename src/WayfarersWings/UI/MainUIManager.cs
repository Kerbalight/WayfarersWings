using BepInEx.Logging;
using UitkForKsp2.API;
using UnityEngine.UIElements;

namespace WayfarersWings.UI;

public class MainUIManager
{
    public static MainUIManager Instance { get; } = new();

    private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("MainUIManager");
    private readonly Dictionary<string, VisualTreeAsset> _templates = new();

    private UIDocument _wingsAppUIDocument = null!;
    public WingsAppWindowController AppWindow { get; private set; } = null!;

    private UIDocument _tooltipUIDocument = null!;
    public TooltipWindowController TooltipWindow { get; private set; } = null!;

    private UIDocument _kerbalUIDocument = null!;
    public KerbalWindowController KerbalWindow { get; private set; } = null!;

    /// <summary>
    /// Loads the UI template from the given VisualTreeAsset found in Addressables.
    /// </summary>
    public void ImportVisualTreeAsset(VisualTreeAsset treeAsset)
    {
        Logger.LogInfo($"Importing VisualTreeAsset {treeAsset.name}");
        _templates.Add(treeAsset.name, treeAsset);
    }

    /// <summary>
    /// Initializes the UI by loading the templates and setting up the UI.
    /// </summary>
    public void Initialize()
    {
        Logger.LogInfo("Initializing UI");

        // Kerbal window
        _kerbalUIDocument = Window.Create(KerbalWindowController.WindowOptions, _templates["KerbalWindow"]);
        KerbalWindow = _kerbalUIDocument.gameObject.AddComponent<KerbalWindowController>();


        // Main window
        _wingsAppUIDocument = Window.Create(WingsAppWindowController.WindowOptions, _templates["WingsAppWindow"]);
        AppWindow = _wingsAppUIDocument.gameObject.AddComponent<WingsAppWindowController>();


        // Tooltip window (should be last)
        _tooltipUIDocument = Window.Create(TooltipWindowController.WindowOptions, _templates["TooltipWindow"]);
        TooltipWindow = _tooltipUIDocument.gameObject.AddComponent<TooltipWindowController>();
    }

    public void ToggleUI(bool? isOpen = null)
    {
        AppWindow.IsWindowOpen = isOpen ?? !AppWindow.IsWindowOpen;
        KerbalWindow.IsWindowOpen = AppWindow.IsWindowOpen && KerbalWindow.KerbalId.HasValue;
    }

    public VisualTreeAsset GetTemplate(string name)
    {
        return _templates[name];
    }
}