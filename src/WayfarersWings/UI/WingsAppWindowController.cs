using BepInEx.Logging;
using KSP.UI.Binding;
using WayfarersWings.Unity.Runtime;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;
using WayfarersWings.Extensions;
using WayfarersWings.Managers;
using WayfarersWings.Managers.Messages;
using WayfarersWings.Models.Session;
using WayfarersWings.UI.Components;
using Logger = UnityEngine.Logger;

namespace WayfarersWings.UI;

/// <summary>
/// Controller for the Main App UI.
/// </summary>
public class WingsAppWindowController : MonoBehaviour
{
    private static ManualLogSource Logger =
        BepInEx.Logging.Logger.CreateLogSource("WayfarerWings.WingsAppWindowController");

    private UIDocument _window;

    public static WindowOptions WindowOptions = new()
    {
        WindowId = "WayfarerWings_AppWindow",
        Parent = null,
        IsHidingEnabled = true,
        DisableGameInputForTextFields = true,
        MoveOptions = new MoveOptions
        {
            IsMovingEnabled = true,
            CheckScreenBounds = false
        }
    };

    // The elements of the window that we need to access
    private VisualElement _root;

    // private ScrollView _content;
    private readonly Dictionary<string, ScrollView> _tabs = new();

    // The backing field for the IsWindowOpen property
    private bool _isWindowOpen;
    private bool _isInitialized;
    private bool _isDirty;

    /// <summary>
    /// The state of the window. Setting this value will open or close the window.
    /// </summary>
    public bool IsWindowOpen
    {
        get => _isWindowOpen;
        set
        {
            _isWindowOpen = value;

            // if (value && !_isInitialized) BuildUI();
            if (value) BuildUI("ribbons");
            if (value && _isDirty) BuildUI("kerbals");

            // Set the display style of the root element to show or hide the window
            _root.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;

            // Update the Flight AppBar button state
            GameObject.Find(WayfarersWingsPlugin.ToolbarFlightButtonID)
                ?.GetComponent<UIValue_WriteBool_Toggle>()
                ?.SetValue(value);

            // Update the OAB AppBar button state
            GameObject.Find(WayfarersWingsPlugin.ToolbarOabButtonID)
                ?.GetComponent<UIValue_WriteBool_Toggle>()
                ?.SetValue(value);
        }
    }

    /// <summary>
    /// Runs when the window is first created, and every time the window is re-enabled.
    /// </summary>
    private void OnEnable()
    {
        // Get the UIDocument component from the game object
        _window = GetComponent<UIDocument>();

        _root = _window.rootVisualElement[0];
        _root.StopMouseEventsPropagation();

        _tabs["ribbons"] = _root.Q<ScrollView>("ribbons-tab");
        _tabs["kerbals"] = _root.Q<ScrollView>("kerbals-tab");

        var debugButton = _root.Q<Button>("debug-button");
        debugButton.clicked += OnDebug;

        _root.Q<Button>("kerbals-button").clicked += () => OnSelectTab("kerbals");

        // Center the window by default
        _root.CenterByDefault();

        IsWindowOpen = false;

        // Get the close button from the window
        var closeButton = _root.Q<Button>("close-button");
        closeButton.clicked += () => IsWindowOpen = false;

        MessageListener.Instance.Subscribe<WingAwardedMessage>(message => BuildUI("kerbals"));
    }

    private void OnDebug()
    {
        OnSelectTab("ribbons");
        foreach (var ribbon in _tabs["ribbons"].Children())
        {
            // Logger.LogDebug($"Ribbon: {ribbon.name}");
        }
    }

    private void OnSelectTab(string tabName)
    {
        Logger.LogDebug($"Selected tab: {tabName}");
        BuildUI(tabName);
        foreach (var tab in _tabs)
        {
            tab.Value.style.display = tab.Key == tabName ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void BuildUI(string tabName)
    {
        if (!_isWindowOpen)
        {
            _isDirty = true;
            return;
        }

        _isDirty = false;
        _tabs[tabName].Clear();

        switch (tabName)
        {
            case "ribbons":
                BuildRibbonsTab();
                break;
            case "kerbals":
                BuildKerbalsTab();
                break;
        }
    }

    private void BuildRibbonsTab()
    {
        var allWings = Core.Instance.WingsPool.Wings;
        foreach (var wing in allWings)
        {
            var ribbon = WingRibbonController.Create();
            _tabs["ribbons"].Add(ribbon.Root);
            ribbon.Bind(wing);
        }
    }

    private void BuildKerbalsTab()
    {
        foreach (var kerbalWings in WingsSessionManager.Instance.KerbalsWings)
        {
            if (!WingsSessionManager.Roster.TryGetKerbalByID(kerbalWings.KerbalId, out var kerbalInfo))
            {
                Logger.LogError("Kerbal not found in roster" + kerbalWings.KerbalId);
                continue;
            }

            var row = KerbalWingsRowController.Create();
            _tabs["kerbals"].Add(row.Root);
            row.Bind(kerbalWings);
        }
    }
}