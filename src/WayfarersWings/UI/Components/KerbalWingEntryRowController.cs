using BepInEx.Logging;
using UnityEngine.UIElements;
using WayfarersWings.Models.Session;
using WayfarersWings.Utility;

namespace WayfarersWings.UI.Components;

public class KerbalWingEntryRowController
{
    private static ManualLogSource
        Logger = BepInEx.Logging.Logger.CreateLogSource("WayfarerWings.WingRowController");

    private VisualElement _root;
    private VisualElement _ribbonsSpace;
    private VisualElement _portrait;
    private Label _name;
    private Label _description;
    private Label _date;

    private KerbalWingEntry _entry;

    public VisualElement Root => _root;

    public KerbalWingEntryRowController(VisualElement root)
    {
        _root = root;
        _ribbonsSpace = _root.Q<VisualElement>("ribbons-space");
        _name = _root.Q<Label>("name");
        _description = _root.Q<Label>("description");
        _date = _root.Q<Label>("date");
    }

    public static KerbalWingEntryRowController Create()
    {
        var template = MainUIManager.Instance.GetTemplate("KerbalWingEntryRow");
        var root = template.Instantiate();
        var controller = new KerbalWingEntryRowController(root);
        root.userData = controller;
        return controller;
    }

    public void Bind(KerbalWingEntry entry)
    {
        _entry = entry;

        _name.text = entry.Wing.DisplayName;
        _description.text = entry.Wing.Description;
        _date.text = DateTimeLogic.FormatUniverseTime(entry.universeTime);

        _ribbonsSpace.Clear();
        var ribbon = WingRibbonController.Create();
        ribbon.DisplayBig = true;
        ribbon.Bind(entry.Wing);
        _ribbonsSpace.Add(ribbon.Root);
    }
}