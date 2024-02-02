using BepInEx.Logging;
using KSP.Game;
using KSP.Messages;
using KSP.Sim.impl;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;
using WayfarersWings.Extensions;
using WayfarersWings.Managers;
using WayfarersWings.Managers.Messages;
using WayfarersWings.Models.Session;
using WayfarersWings.UI.Components;
using WayfarersWings.UI.Localization;
using WayfarersWings.Utility;

namespace WayfarersWings.UI;

/// <summary>
/// Mission summary shown on vessel recovery.
/// </summary>
public class MissionSummaryWindowController : MonoBehaviour
{
    private static readonly ManualLogSource Logger =
        BepInEx.Logging.Logger.CreateLogSource("WayfarersWings.MissionSummaryWindowController");

    private UIDocument _window = null!;

    public static WindowOptions WindowOptions = new()
    {
        WindowId = "WayfarerWings_MissionSummaryWindow",
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
    private VisualElement _content = null!;

    private bool _isWindowOpen;

    public bool IsWindowOpen
    {
        get => _isWindowOpen;
        set
        {
            _isWindowOpen = value;
            _root.style.display = _isWindowOpen ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    /// <summary>
    /// Prepares the common Mission Window layout.
    /// </summary>
    private void OnEnable()
    {
        // Get the UIDocument component from the game object
        _window = GetComponent<UIDocument>();

        _root = _window.rootVisualElement[0];
        _root.StopMouseEventsPropagation();
        _root.CenterByDefault(); // TODO: make this configurable

        _root.Q<Label>("title").text = LocalizedStrings.MissionAchievementsTitle.ToString().ToUpper();

        // Content
        _content = _root.Q<VisualElement>("kerbals-container");

        // Get the close button from the window
        var closeButton = _root.Q<Button>("close-button");
        closeButton.clicked += () => IsWindowOpen = false;

        IsWindowOpen = false;

        // Message trigger
        Core.Messages.PersistentSubscribe<WingMissionCompletedMessage>(OnMissionCompleted);
    }

    /// <summary>
    /// When the mission is completed, show the mission summary window.
    /// We don't use `VesselRecoveredMessage` because WW wings may not be calculated.
    /// </summary>
    private void OnMissionCompleted(MessageCenterMessage message)
    {
        if (!Settings.ShowFlightReportSummary.Value) return;

        var missionCompletedMessage = (WingMissionCompletedMessage)message;
        var vessel = missionCompletedMessage.Vessel;
        if (vessel == null)
        {
            Logger.LogError("Vessel is null, cannot show mission summary window.");
            return;
        }

        if (vessel.GlobalId != Core.ActiveVessel?.GlobalId)
        {
            Logger.LogDebug($"Vessel {vessel.GlobalId} is not the active vessel, not showing mission summary window.");
            return;
        }

        // Display if there are kerbals in the vessel
        BuildAndShowVesselSummary(vessel);
    }

    private void BuildAndShowVesselSummary(VesselComponent vessel)
    {
        var kerbalInfos = WingsSessionManager.Roster.GetAllKerbalsInVessel(vessel.GlobalId);

        _content.Clear();
        foreach (var kerbalInfo in kerbalInfos)
        {
            var profile = WingsSessionManager.Instance.GetKerbalProfile(kerbalInfo.Id);
            var row = KerbalProfileRowController.Create();
            _content.Add(row.Root);
            row.ShowLastMissionOnly = true;
            row.Bind(profile);
        }

        // If there are kerbals in the vessel, show the window
        if (kerbalInfos.Count > 0) IsWindowOpen = true;
    }
}