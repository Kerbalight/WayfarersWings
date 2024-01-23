using BepInEx.Logging;
using UnityEngine.UIElements;
using WayfarersWings.Models.Session;

namespace WayfarersWings.UI.Components;

public class WingRowController
{
    private static ManualLogSource
        Logger = BepInEx.Logging.Logger.CreateLogSource("WayfarerWings.WingRowController");

    private VisualElement _root;
    private VisualElement _ribbonsSpace;
    private VisualElement _portrait;
    private Label _name;
    private Label _description;

    private KerbalWingEntry _kerbalWingEntry;

    public VisualElement Root => _root;

    public WingRowController(VisualElement root)
    {
        _root = root;
        _ribbonsSpace = _root.Q<VisualElement>("ribbons-space");
        _name = _root.Q<Label>("name");
        _description = _root.Q<Label>("description");
    }

    public static WingRowController Create()
    {
        var template = MainUIManager.Instance.GetTemplate("WingRow");
        var root = template.Instantiate();
        var controller = new WingRowController(root);
        root.userData = controller;
        return controller;
    }

    public void Bind(KerbalWingEntry entry)
    {
        _kerbalWingEntry = entry;

        _name.text = entry.Wing.DisplayName;
        _description.text = entry.Wing.Description;

        _ribbonsSpace.Clear();
        var ribbon = WingRibbonController.Create();
        ribbon.DisplayBig = true;
        ribbon.Bind(entry.Wing);
        _ribbonsSpace.Add(ribbon.Root);
    }
}