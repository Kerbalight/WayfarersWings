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
using WayfarersWings.UI.Localization;
using WayfarersWings.UI.Logic;
using Logger = UnityEngine.Logger;

namespace WayfarersWings.UI;

/// <summary>
/// Controller for the Main App UI.
/// </summary>
public class WingsAppWindowController : MonoBehaviour
{
    private static readonly ManualLogSource Logger =
        BepInEx.Logging.Logger.CreateLogSource("WayfarersWings.WingsAppWindowController");

    private UIDocument _window = null!;

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
    private VisualElement _root = null!;

    public VisualElement Root => _root;

    // private ScrollView _content;
    private const string DefaultTab = "kerbals";
    private string _selectedTab = DefaultTab;
    private readonly Dictionary<string, ScrollView> _tabs = new();
    private readonly Dictionary<string, Button> _tabButtons = new();
    private VisualElement _kerbalsTabMenu = null!;
    private DropdownField _sortDropdown = null!;
    private Button _sortDirectionButton = null!;
    private TextField _nameSearchField = null!;
    private KerbalProfileQuery.Query _kerbalQuery = new();

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
            if (value && (!_isInitialized || _isDirty)) OnSelectTab(_selectedTab);

            if (!value && MainUIManager.Instance.KerbalWindow != null)
            {
                MainUIManager.Instance.KerbalWindow.IsWindowOpen = false;
            }

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

    public float Width => _root.resolvedStyle.width;

    /// <summary>
    /// Runs when the window is first created, and every time the window is re-enabled.
    /// </summary>
    private void OnEnable()
    {
        // Get the UIDocument component from the game object
        _window = GetComponent<UIDocument>();

        _root = _window.rootVisualElement[0];
        _root.StopMouseEventsPropagation();

        // Tab buttons
        _tabs["ribbons"] = _root.Q<ScrollView>("ribbons-tab");
        _tabs["kerbals"] = _root.Q<ScrollView>("kerbals-tab");

        _tabButtons["ribbons"] = _root.Q<Button>("ribbons-button");
        _tabButtons["ribbons"].clicked += () => OnSelectTab("ribbons");

        _tabButtons["kerbals"] = _root.Q<Button>("kerbals-button");
        _tabButtons["kerbals"].clicked += () => OnSelectTab("kerbals");

        // Kerbals Menu
        _kerbalsTabMenu = _root.Q<VisualElement>("kerbals-tab-menu");
        _sortDropdown = _kerbalsTabMenu.Q<DropdownField>("sort-by-dropdown");
        _sortDropdown.choices = KerbalProfileQuery.SortOptions;
        // Done even in `Localize()` because
        _sortDropdown.value = _kerbalQuery.GetSortChoice();
        _sortDropdown.RegisterValueChangedCallback(OnKerbalsSortChanged);
        _sortDirectionButton = _kerbalsTabMenu.Q<Button>("sort-by-direction");
        _sortDirectionButton.clicked += OnKerbalsDirectionChanged;
        AlignButtonSortDirection();
        _nameSearchField = _kerbalsTabMenu.Q<TextField>("search-name-field");
        _nameSearchField.RegisterValueChangedCallback(OnKerbalsNameSearchChanged);

        // Center the window by default
        _root.CenterByDefault();

        Localize();

        IsWindowOpen = false;

        // Get the close button from the window
        var closeButton = _root.Q<Button>("close-button");
        closeButton.clicked += () => IsWindowOpen = false;

        MessageListener.Subscribe<WingAwardedMessage>(message => BuildUI("kerbals"));
        MessageListener.Subscribe<WingRevokedMessage>(message => BuildUI("kerbals"));
    }

    private void OnSelectTab(string tabName)
    {
        Logger.LogDebug($"Selected tab: {tabName}");
        _selectedTab = tabName;
        BuildUI(_selectedTab);
        foreach (var tab in _tabs)
        {
            _tabButtons[tab.Key].RemoveFromClassList("tabs-menu__item--selected");
            tab.Value.style.display = tab.Key == tabName ? DisplayStyle.Flex : DisplayStyle.None;
        }

        _tabButtons[tabName].AddToClassList("tabs-menu__item--selected");
    }

    private void Localize()
    {
        _root.Q<Label>("title").text = LocalizedStrings.AchievementsTitle.ToString().ToUpper();

        _tabButtons["ribbons"].text = LocalizedStrings.AchievementsTabRibbons;
        _tabButtons["kerbals"].text = LocalizedStrings.AchievementsTabKerbals;

        _sortDropdown.choices = KerbalProfileQuery.SortOptions;
        _sortDropdown.value = _kerbalQuery.GetSortChoice();
    }

    private void BuildUI(string tabName)
    {
        if (!_isWindowOpen)
        {
            _isDirty = true;
            return;
        }

        _isInitialized = true;
        _isDirty = false;
        _tabs[tabName].Clear();
        _kerbalsTabMenu.style.display = tabName == "kerbals" ? DisplayStyle.Flex : DisplayStyle.None;

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
        var kerbalProfiles = WingsSessionManager.Instance.GetAllKerbalsProfiles();
        _kerbalQuery.Apply(kerbalProfiles);

        foreach (var kerbalProfile in kerbalProfiles)
        {
            var row = KerbalProfileRowController.Create();
            _tabs["kerbals"].Add(row.Root);
            row.Bind(kerbalProfile);
        }

        // Refresh localizations
        Localize();

        Logger.LogInfo($"Refreshed Kerbals tab with {kerbalProfiles.Count} kerbals.");
    }

    // Search & Sort
    private void OnKerbalsSortChanged(ChangeEvent<string> sortOption)
    {
        var index = _sortDropdown.choices.IndexOf(sortOption.newValue);
        _kerbalQuery.SetSort(index);
        Refresh();
    }

    private void OnKerbalsDirectionChanged()
    {
        _kerbalQuery.direction = _kerbalQuery.direction == KerbalProfileQuery.Direction.Ascending
            ? KerbalProfileQuery.Direction.Descending
            : KerbalProfileQuery.Direction.Ascending;
        AlignButtonSortDirection();
        Refresh();
    }

    private void AlignButtonSortDirection()
    {
        _sortDirectionButton.style.rotate = _kerbalQuery.direction == KerbalProfileQuery.Direction.Ascending
            ? new StyleRotate(new UnityEngine.UIElements.Rotate(180))
            : new StyleRotate(new UnityEngine.UIElements.Rotate(0));
    }

    private void OnKerbalsNameSearchChanged(ChangeEvent<string> evt)
    {
        _kerbalQuery.nameSearch = evt.newValue;
        Refresh();
    }

    public void Refresh()
    {
        BuildUI(_selectedTab);
    }
}