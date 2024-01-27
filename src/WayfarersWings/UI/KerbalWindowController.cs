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
    private static ManualLogSource Logger =
        BepInEx.Logging.Logger.CreateLogSource("WayfarerWings.KerbalWindowController");

    private UIDocument _window;

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
    private VisualElement _root;

    // private ScrollView _content;
    private Label _nameLabel;
    private ScrollView _ribbonsView;

    // Summary
    private Label _totalMissionTimeLabel;
    private Label _totalMissionTimeValueLabel;
    private Label _missionsLabel;
    private Label _missionsValueLabel;
    private Label _totalEvaTimeLabel;
    private Label _totalEvaTimeValueLabel;
    private Label _statusLabel;
    private Label _statusValueLabel;

    private VisualElement _statusIcon;

    // Award
    private Foldout _awardFoldout;
    private List<Wing> _awardableWings = [];
    private ListView _awardablesList;
    private Button _awardConfirmButton;
    private TextField _searchAwardablesField;
    private Wing? _selectedWing;

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
            if (value) BuildUI();
            else KerbalId = null;

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

    public void SelectKerbal(IGGuid kerbalId)
    {
        KerbalId = kerbalId;
        WingsSessionManager.Roster.TryGetKerbalByID(kerbalId, out _kerbalInfo);
        if (IsWindowOpen) BuildUI();
        else IsWindowOpen = true;
    }

    private void OnConfirmAward()
    {
        if (_selectedWing == null || _kerbalInfo == null) return;
        WingsSessionManager.Instance.Award(_selectedWing, _kerbalInfo);
        // Logger.LogDebug($"Awarding {_selectedWing.DisplayName} to {_kerbalInfo?.Attributes.GetFullName()}");
    }

    private void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        // Award foldout
        _awardFoldout.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue)
            {
                _root.AddToClassList("root--expanded");
            }
            else
            {
                _root.RemoveFromClassList("root--expanded");
            }
        });
        _awardFoldout.value = false;

        // Search
        _searchAwardablesField.RegisterValueChangedCallback(evt =>
        {
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

            _awardablesList.selectedIndex = -1;
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
            var selectionArray = selection as int[] ?? selection.ToArray();
            if (selectionArray[0] < 0) selectionArray = Array.Empty<int>();
            _selectedWing = selectionArray.Any() ? Core.Instance.WingsPool.Wings[selectionArray[0]] : null;

            Logger.LogDebug($"Selected wing {_selectedWing?.DisplayName}");
            if (_selectedWing != null)
            {
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

        // Align window
        var appWindow = MainUIManager.Instance.AppWindow;
        var appWindowPosition = appWindow.Root.transform.position;
        _root.transform.position = new Vector3(appWindowPosition.x + 10 + appWindow.Width,
            appWindowPosition.y, appWindowPosition.z);

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
        BuildUI();
    }
}