using BepInEx.Logging;
using I2.Loc;
using KSP.Game;
using UnityEngine;
using UnityEngine.UIElements;
using WayfarersWings.Models.Session;
using WayfarersWings.UI.Localization;
using WayfarersWings.Utility;

namespace WayfarersWings.UI.Components;

public class KerbalProfileRowController
{
    private static readonly ManualLogSource
        Logger = BepInEx.Logging.Logger.CreateLogSource("WayfarerWings.KerbalWingsRowController");

    private readonly VisualElement _root;
    private readonly VisualElement _ribbonsSpace;
    private readonly VisualElement _portrait;
    private readonly VisualElement _status;
    private readonly Label _name;
    private readonly Label _missionsLabel;
    private readonly Button _starButton;
    private readonly Button _infoButton;
    private readonly Button _headerButton;

    private KerbalProfile? _kerbalProfile;

    public VisualElement Root => _root;

    /// <summary>
    /// Allows to show only the wings that were unlocked since the last launch.
    /// Useful for the mission report.
    /// </summary>
    public bool ShowLastMissionOnly { get; set; } = false;

    public KerbalProfileRowController(VisualElement root)
    {
        _root = root;
        _ribbonsSpace = _root.Q<VisualElement>("ribbons-space");
        _portrait = _root.Q<VisualElement>("portrait");
        _name = _root.Q<Label>("name");
        _missionsLabel = _root.Q<Label>("missions-label");
        _starButton = _root.Q<Button>("star-button");
        _infoButton = _root.Q<Button>("info-button");
        _headerButton = _root.Q<Button>("header-button");
        _status = _root.Q<VisualElement>("status");

        _headerButton.clicked += OnInfoClicked;
        _infoButton.clicked += OnInfoClicked;
        _starButton.clicked += OnStarClicked;
    }

    private void OnInfoClicked()
    {
        Logger.LogDebug("Info clicked");
        MainUIManager.Instance.KerbalWindow.SelectKerbal(_kerbalProfile!.kerbalId);
    }

    private void OnStarClicked()
    {
        _kerbalProfile!.isStarred = !_kerbalProfile.isStarred;
        _starButton.style.unityBackgroundImageTintColor =
            _kerbalProfile.isStarred ? Colors.LedGreen : Color.white;
    }

    public static KerbalProfileRowController Create()
    {
        var template = MainUIManager.Instance.GetTemplate("KerbalProfileRow");
        var root = template.Instantiate();
        var controller = new KerbalProfileRowController(root);
        root.userData = controller;
        return controller;
    }

    public void Bind(KerbalProfile kerbalProfile)
    {
        _kerbalProfile = kerbalProfile;

        if (!WingsSessionManager.Roster.TryGetKerbalByID(kerbalProfile.kerbalId, out var kerbalInfo) ||
            kerbalInfo == null)
        {
            Logger.LogError("Kerbal not found in roster");
            return;
        }

        if (kerbalInfo.Portrait != null && kerbalInfo.Portrait.texture != null)
        {
            _portrait.style.backgroundImage = new StyleBackground(kerbalInfo.Portrait.texture);
        }

        _name.text = kerbalInfo.Attributes.GetFullName();
        _missionsLabel.text = LocalizationManager.GetTranslation(LocalizedStrings.KerbalMissionsCount,
            new object[] { kerbalProfile.missionsCount });

        _starButton.style.unityBackgroundImageTintColor =
            _kerbalProfile.isStarred ? Colors.LedGreen : Color.white;

        KerbalStatusElement.SetStatus(_status, kerbalProfile);


        // Display only the wings that were unlocked since the last launch
        // if the option is enabled
        var entries = ShowLastMissionOnly
            ? kerbalProfile.GetLastMissionEntries()
            : kerbalProfile.Entries;

        _ribbonsSpace.Clear();
        foreach (var wingEntry in entries)
        {
            if (wingEntry.isSuperseeded) continue;
            var ribbon = WingRibbonController.Create();
            ribbon.Bind(wingEntry.Wing);
            _ribbonsSpace.Add(ribbon.Root);
        }
    }
}