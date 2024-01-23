using BepInEx.Logging;
using KSP.Game;
using UnityEngine;
using UnityEngine.UIElements;
using WayfarersWings.Models.Session;
using WayfarersWings.Utility;

namespace WayfarersWings.UI.Components;

public class KerbalWingsRowController
{
    private static ManualLogSource
        Logger = BepInEx.Logging.Logger.CreateLogSource("WayfarerWings.KerbalWingsRowController");

    private VisualElement _root;
    private VisualElement _ribbonsSpace;
    private VisualElement _portrait;
    private Label _name;
    private Label _missionsLabel;
    private Button _starButton;
    private Button _infoButton;
    private Button _headerButton;

    private KerbalWingEntries _kerbalWingEntries;

    public VisualElement Root => _root;

    public KerbalWingsRowController(VisualElement root)
    {
        _root = root;
        _ribbonsSpace = _root.Q<VisualElement>("ribbons-space");
        _portrait = _root.Q<VisualElement>("portrait");
        _name = _root.Q<Label>("name");
        _missionsLabel = _root.Q<Label>("missions-label");
        _starButton = _root.Q<Button>("star-button");
        _infoButton = _root.Q<Button>("info-button");
        _headerButton = _root.Q<Button>("header-button");

        _headerButton.clicked += OnInfoClicked;
        _infoButton.clicked += OnInfoClicked;
        _starButton.clicked += OnStarClicked;
    }

    private void OnInfoClicked()
    {
        Logger.LogDebug("Info clicked");
    }

    private void OnStarClicked()
    {
        _kerbalWingEntries.IsStarred = !_kerbalWingEntries.IsStarred;
        _starButton.style.unityBackgroundImageTintColor =
            _kerbalWingEntries.IsStarred ? Colors.LedGreen : Color.white;
    }

    public static KerbalWingsRowController Create()
    {
        var template = MainUIManager.Instance.GetTemplate("KerbalWingsRow");
        var root = template.Instantiate();
        var controller = new KerbalWingsRowController(root);
        root.userData = controller;
        return controller;
    }

    public void Bind(KerbalWingEntries kerbalWingEntries)
    {
        _kerbalWingEntries = kerbalWingEntries;

        if (!WingsSessionManager.Roster.TryGetKerbalByID(kerbalWingEntries.KerbalId, out var kerbalInfo))
        {
            Logger.LogError("Kerbal not found in roster");
            return;
        }

        Logger.LogDebug("Binding kerbal " + kerbalInfo.Attributes.GetFullName());

        _portrait.style.backgroundImage = new StyleBackground(kerbalInfo.Portrait.texture);
        _name.text = kerbalInfo.Attributes.GetFullName();
        _missionsLabel.text = kerbalWingEntries.MissionsCount + " missions"; // TODO i18n
        _starButton.style.unityBackgroundImageTintColor =
            _kerbalWingEntries.IsStarred ? Colors.LedGreen : Color.white;

        _ribbonsSpace.Clear();
        foreach (var wingEntry in kerbalWingEntries.Entries)
        {
            if (wingEntry.isSuperseeded) continue;
            var ribbon = WingRibbonController.Create();
            ribbon.Bind(wingEntry.Wing);
            _ribbonsSpace.Add(ribbon.Root);
        }
    }
}