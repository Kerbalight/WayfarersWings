using BepInEx.Logging;
using KSP.Game;
using UnityEngine.UIElements;
using WayfarersWings.Models.Session;

namespace WayfarersWings.UI.Components;

public class KerbalWingsRowController
{
    private static ManualLogSource
        Logger = BepInEx.Logging.Logger.CreateLogSource("WayfarerWings.KerbalWingsRowController");

    private VisualElement _root;
    private VisualElement _ribbonsSpace;
    private VisualElement _portrait;
    private Label _name;

    public VisualElement Root => _root;

    public KerbalWingsRowController(VisualElement root)
    {
        _root = root;
        _ribbonsSpace = _root.Q<VisualElement>("ribbons-space");
        _portrait = _root.Q<VisualElement>("portrait");
        _name = _root.Q<Label>("name");
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
        if (!WingsSessionManager.Roster.TryGetKerbalByID(kerbalWingEntries.KerbalId, out var kerbalInfo))
        {
            Logger.LogError("Kerbal not found in roster");
            return;
        }

        Logger.LogDebug("Binding kerbal " + kerbalInfo.Attributes.GetFullName());

        _portrait.style.backgroundImage = new StyleBackground(kerbalInfo.Portrait.texture);
        _name.text = kerbalInfo.Attributes.GetFullName();

        _ribbonsSpace.Clear();
        foreach (var wingEntry in kerbalWingEntries.Entries)
        {
            var ribbon = WingRibbonController.Create();
            ribbon.Bind(wingEntry.Wing);
            _ribbonsSpace.Add(ribbon.Root);
        }
    }
}