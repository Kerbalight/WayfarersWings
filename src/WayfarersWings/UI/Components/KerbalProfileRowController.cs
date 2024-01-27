using BepInEx.Logging;
using KSP.Game;
using UnityEngine;
using UnityEngine.UIElements;
using WayfarersWings.Models.Session;
using WayfarersWings.Utility;

namespace WayfarersWings.UI.Components;

public class KerbalProfileRowController
{
    private static ManualLogSource
        Logger = BepInEx.Logging.Logger.CreateLogSource("WayfarerWings.KerbalWingsRowController");

    private VisualElement _root;
    private VisualElement _ribbonsSpace;
    private VisualElement _portrait;
    private VisualElement _status;
    private Label _name;
    private Label _missionsLabel;
    private Button _starButton;
    private Button _infoButton;
    private Button _headerButton;

    private KerbalProfile _kerbalProfile;

    public VisualElement Root => _root;

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
        MainUIManager.Instance.KerbalWindow.SelectKerbal(_kerbalProfile.kerbalId);
    }

    private void OnStarClicked()
    {
        _kerbalProfile.isStarred = !_kerbalProfile.isStarred;
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

        if (!WingsSessionManager.Roster.TryGetKerbalByID(kerbalProfile.kerbalId, out var kerbalInfo))
        {
            Logger.LogError("Kerbal not found in roster");
            return;
        }

        Logger.LogDebug("Binding kerbal " + kerbalInfo.Attributes.GetFullName());

        _portrait.style.backgroundImage = new StyleBackground(kerbalInfo.Portrait.texture);
        _name.text = kerbalInfo.Attributes.GetFullName();
        _missionsLabel.text = kerbalProfile.missionsCount + " missions"; // TODO i18n
        _starButton.style.unityBackgroundImageTintColor =
            _kerbalProfile.isStarred ? Colors.LedGreen : Color.white;

        KerbalStatusElement.SetStatus(_status, kerbalProfile);

        _ribbonsSpace.Clear();
        foreach (var wingEntry in kerbalProfile.Entries)
        {
            if (wingEntry.isSuperseeded) continue;
            var ribbon = WingRibbonController.Create();
            ribbon.Bind(wingEntry.Wing);
            _ribbonsSpace.Add(ribbon.Root);
        }
    }
}