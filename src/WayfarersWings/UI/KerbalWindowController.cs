using BepInEx.Logging;
using I2.Loc;
using KSP.Game;
using KSP.Messages;
using KSP.Sim.impl;
using KSP.UI.Binding;
using WayfarersWings.Unity.Runtime;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;
using WayfarersWings.Extensions;
using WayfarersWings.Managers;
using WayfarersWings.Managers.Messages;
using WayfarersWings.Models.Session;
using WayfarersWings.Models.Wings;
using WayfarersWings.UI.Components;
using WayfarersWings.UI.Localization;
using WayfarersWings.Utility.Serialization;
using Logger = UnityEngine.Logger;

namespace WayfarersWings.UI;

/// <summary>
/// Controller for the Single Kerbal window.
/// </summary>
public class KerbalWindowController : MonoBehaviour
{
    private static readonly ManualLogSource Logger =
        BepInEx.Logging.Logger.CreateLogSource("WayfarerWings.KerbalWindowController");

    private UIDocument _window = null!;

    public static WindowOptions WindowOptions = new()
    {
        WindowId = "WayfarerWings_KerbalWindow",
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

    // private ScrollView _content;
    private Label _nameLabel = null!;
    private ScrollView _ribbonsView = null!;

    // Summary
    private Label _totalMissionTimeLabel = null!;
    private Label _totalMissionTimeValueLabel = null!;
    private Label _missionsLabel = null!;
    private Label _missionsValueLabel = null!;
    private Label _totalEvaTimeLabel = null!;
    private Label _totalEvaTimeValueLabel = null!;
    private Label _statusLabel = null!;
    private Label _statusValueLabel = null!;

    private VisualElement _statusIcon = null!;

    // Award
    private Foldout _awardFoldout = null!;
    private readonly List<Wing> _awardableWings = [];
    private ListView _awardablesList = null!;
    private Button _awardConfirmButton = null!;
    private TextField _searchAwardablesField = null!;
    private Wing? _selectedWing = null!;

    // The backing field for the IsWindowOpen property
    private bool _isWindowOpen;
    private bool _isInitialized;
    private bool _isDirty;

    public IGGuid? KerbalId { get; private set; }
    private KerbalInfo? _kerbalInfo;

    public bool IsWindowOpen
    {
        get => _isWindowOpen;
        set
        {
            if (value || _isDirty) BuildUI();
            else KerbalId = null;

            _isWindowOpen = value;

            _root.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
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

        _nameLabel = _root.Q<Label>("name-label");
        // _detailLabel = _root.Q<Label>("detail-label");
        _ribbonsView = _root.Q<ScrollView>("ribbons-view");

        // Summary
        _missionsLabel = _root.Q<Label>("missions-label");
        _missionsValueLabel = _root.Q<Label>("missions-value");
        _totalEvaTimeLabel = _root.Q<Label>("total-eva-time-label");
        _totalEvaTimeValueLabel = _root.Q<Label>("total-eva-time-value");
        _totalMissionTimeLabel = _root.Q<Label>("total-mission-time-label");
        _totalMissionTimeValueLabel = _root.Q<Label>("total-mission-time-value");
        _statusLabel = _root.Q<Label>("status-label");
        _statusValueLabel = _root.Q<Label>("status-value");
        _statusIcon = _root.Q<VisualElement>("status-icon");

        _awardFoldout = _root.Q<Foldout>("award-foldout");
        _awardablesList = _root.Q<ListView>("awardables-list");
        _awardConfirmButton = _root.Q<Button>("award-confirm-button");
        _searchAwardablesField = _root.Q<TextField>("search-awardables-field");

        IsWindowOpen = false;

        // Localization
        SetStarText(_missionsLabel, LocalizedStrings.MissionsCompleted);
        SetStarText(_totalEvaTimeLabel, LocalizedStrings.TotalEvaTime);
        SetStarText(_totalMissionTimeLabel, LocalizedStrings.TotalMissionTime);
        SetStarText(_statusLabel, LocalizedStrings.Status);
        _window.EnableLocalization();

        // Get the close button from the window
        var closeButton = _root.Q<Button>("close-button");
        closeButton.clicked += () => IsWindowOpen = false;

        MessageListener.Instance.Subscribe<WingAwardedMessage>(OnWingAwarded);
        MessageListener.Instance.Subscribe<WingRevokedMessage>(OnWingRevoked);
    }

    private void OnWingAwarded(MessageCenterMessage message)
    {
        var awardedMessage = message as WingAwardedMessage;
        if (awardedMessage?.KerbalInfo.Id != KerbalId) return;
        Refresh();
    }

    private void OnWingRevoked(MessageCenterMessage message)
    {
        var revokedMessage = message as WingRevokedMessage;
        if (revokedMessage?.KerbalId != KerbalId) return;
        Refresh();
    }

    private void SetStarText(Label label, string text)
    {
        label.text = "<color=#595DD5>*</color> " + text;
    }

    /// <summary>
    /// Public API used by the main window to open the window and display
    /// Kerbal information.
    /// </summary>
    public void SelectKerbal(IGGuid kerbalId)
    {
        KerbalId = kerbalId;
        WingsSessionManager.Roster.TryGetKerbalByID(kerbalId, out _kerbalInfo);
        if (IsWindowOpen) BuildUI();
        else
        {
            IsWindowOpen = true;
            AlignWindowToParent();
        }
    }

    private void OnConfirmAward()
    {
        if (_selectedWing == null || _kerbalInfo == null) return;
        WingsSessionManager.Instance.Award(_selectedWing, _kerbalInfo);
        _awardablesList.ClearSelection();
    }

    /// <summary>
    /// Setups the UI elements bound to the Award foldout.
    /// </summary>
    private void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        // Award foldout
        _awardFoldout.value = false;

        // Search
        _searchAwardablesField.RegisterValueChangedCallback(evt =>
        {
            // We need to reset the selected index before refreshing the list
            _awardablesList.selectedIndex = -1;

            _awardableWings.Clear();
            if (string.IsNullOrWhiteSpace(evt.newValue))
            {
                _awardableWings.AddRange(Core.Instance.WingsPool.Wings);
            }
            else
            {
                _awardableWings.AddRange(Core.Instance.WingsPool.Wings
                    .Where(wing => wing.DisplayName.ToLower().Contains(evt.newValue?.ToLower() ?? ""))
                    .ToList());
            }

            _awardablesList.RefreshItems();
        });

        // List
        _awardablesList.fixedItemHeight = 55;
        _awardableWings.AddRange(Core.Instance.WingsPool.Wings);
        _awardablesList.itemsSource = _awardableWings;
        _awardablesList.makeItem = () =>
        {
            var controller = KerbalWingEntryRowController.Create();
            controller.SetEllipsis(true);
            return controller.Root;
        };

        _awardablesList.bindItem = (visualElement, i) =>
        {
            var row = visualElement.userData as KerbalWingEntryRowController;
            if (_awardablesList.itemsSource[i] is not Wing wing) return;
            row?.Bind(wing);
        };

        // See:
        // https://forum.unity.com/threads/i-cant-select-listview-items-on-uielements-runtime.870013/
        _awardablesList.itemsChosen += items => { };
        _awardablesList.selectedIndicesChanged += selection =>
        {
            _selectedWing = _awardablesList.selectedItem as Wing;

            if (_selectedWing != null)
            {
                Logger.LogDebug($"Selected wing {_selectedWing?.DisplayName}");
                _awardConfirmButton.style.display = DisplayStyle.Flex;
                _awardConfirmButton.text = LocalizedStrings.AwardToKerbal;
            }
            else
            {
                _awardConfirmButton.style.display = DisplayStyle.None;
            }
        };
        _awardConfirmButton.style.display = DisplayStyle.None;
        _awardConfirmButton.clicked += OnConfirmAward;
    }

    /// <summary>
    /// Actually I don't like UI behaviors that changes the window size, so
    /// this is currently unused.
    /// </summary>
    private void InitializeWindowExpandedOnFoldout()
    {
        _awardFoldout.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue)
                _root.AddToClassList("root--expanded");
            else
                _root.RemoveFromClassList("root--expanded");
        });
    }

    /// <summary>
    /// When the window is opened, we align it to the right of the main window.
    /// </summary>
    private void AlignWindowToParent()
    {
        var appWindow = MainUIManager.Instance.AppWindow;
        var appWindowPosition = appWindow.Root.transform.position;
        _root.transform.position = new Vector3(appWindowPosition.x + 10 + appWindow.Width,
            appWindowPosition.y, appWindowPosition.z);
    }

    private void BuildUI()
    {
        Initialize();

        var kerbalId = KerbalId;
        if (!kerbalId.HasValue) return;

        if (_kerbalInfo == null)
        {
            Logger.LogWarning("Kerbal not found in roster, not opening");
            return;
        }

        var profile = WingsSessionManager.Instance.GetKerbalProfile(kerbalId.Value);

        // Set data
        _nameLabel.text = _kerbalInfo?.Attributes.GetFullName() ?? "N/A";
        _missionsValueLabel.text = profile.missionsCount.ToString();
        _totalEvaTimeValueLabel.text = GameTimeSpan.FromSeconds(profile.TotalEvaTime).Format();
        _totalMissionTimeValueLabel.text = GameTimeSpan.FromSeconds(profile.totalMissionTime).Format();

        KerbalStatusElement.SetText(_statusValueLabel, profile);
        KerbalStatusElement.SetStatus(_statusIcon, profile);

        _ribbonsView.Clear();
        foreach (var wingEntry in profile.Entries)
        {
            if (wingEntry.isSuperseeded) continue;
            var row = KerbalWingEntryRowController.Create();
            row.Bind(wingEntry);
            _ribbonsView.Add(row.Root);
        }
    }

    public void Refresh()
    {
        if (_isWindowOpen) BuildUI();
        else _isDirty = true;
    }
}