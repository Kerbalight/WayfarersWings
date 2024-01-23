using BepInEx.Logging;
using KSP.Game;
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
using WayfarersWings.UI.Components;
using WayfarersWings.UI.Localization;
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
    private Label _detailLabel;
    private ScrollView _ribbonsView;

    // The backing field for the IsWindowOpen property
    private bool _isWindowOpen;
    private bool _isDirty;

    public IGGuid? KerbalId { get; private set; }

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
        _detailLabel = _root.Q<Label>("detail-label");
        _ribbonsView = _root.Q<ScrollView>("ribbons-view");

        IsWindowOpen = false;

        // Get the close button from the window
        var closeButton = _root.Q<Button>("close-button");
        closeButton.clicked += () => IsWindowOpen = false;

        MessageListener.Instance.Subscribe<WingAwardedMessage>(message =>
        {
            var awardedMessage = message as WingAwardedMessage;
            if (awardedMessage?.KerbalInfo.Id != KerbalId) return;
            Refresh();
        });
    }

    public void SelectKerbal(IGGuid kerbalId)
    {
        KerbalId = kerbalId;
        if (IsWindowOpen) BuildUI();
        else IsWindowOpen = true;
    }

    private void BuildUI()
    {
        var kerbalId = KerbalId;
        if (!kerbalId.HasValue) return;

        if (!WingsSessionManager.Roster.TryGetKerbalByID(kerbalId.Value, out var kerbalInfo))
        {
            Logger.LogWarning("Kerbal not found in roster, not opening");
        }

        // Align window
        var appWindow = MainUIManager.Instance.AppWindow;
        var appWindowPosition = appWindow.Root.transform.position;
        _root.transform.position = new Vector3(appWindowPosition.x + 10 + appWindow.Width,
            appWindowPosition.y, appWindowPosition.z);

        // Set data
        _nameLabel.text = kerbalInfo.Attributes.GetFullName();
        _detailLabel.text = "10 Missions";

        var entries = WingsSessionManager.Instance.GetKerbalWings(kerbalInfo.Id);

        _ribbonsView.Clear();
        foreach (var wingEntry in entries.Entries)
        {
            if (wingEntry.isSuperseeded) continue;
            var row = WingRowController.Create();
            row.Bind(wingEntry);
            _ribbonsView.Add(row.Root);
        }
    }

    public void Refresh()
    {
        BuildUI();
    }
}