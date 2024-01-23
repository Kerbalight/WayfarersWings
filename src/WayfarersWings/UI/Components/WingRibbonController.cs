using BepInEx.Logging;
using KSP.Game;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using WayfarersWings.Models.Wings;
using WayfarersWings.Utility;

namespace WayfarersWings.UI.Components;

public class WingRibbonController
{
    private static ManualLogSource
        Logger = BepInEx.Logging.Logger.CreateLogSource("WayfarerWings.WingRibbonController");

    private VisualElement _root;
    private VisualElement _ribbonContainer;
    private VisualElement _base;
    private List<VisualElement> _layers;

    public bool DisplayBig;

    public WingRibbonController(VisualElement root)
    {
        _root = root;
        _ribbonContainer = _root.Q<VisualElement>("ribbon-container");
        _base = _root.Q<VisualElement>("base");
        _layers = new List<VisualElement>();

        DisplayBig = Settings.ShowAlwaysBigRibbons.Value;

        _root.Q<VisualElement>("test").style.display = DisplayStyle.None;
    }

    public VisualElement Root
    {
        get => _root;
    }

    public static WingRibbonController Create()
    {
        var template = MainUIManager.Instance.GetTemplate("WingRibbon");
        var root = template.Instantiate();
        var controller = new WingRibbonController(root);
        root.userData = controller;
        return controller;
    }

    public void Bind(Wing wing)
    {
        foreach (var layer in _layers)
        {
            _ribbonContainer.Remove(layer);
        }

        if (DisplayBig) _ribbonContainer.RemoveFromClassList("ribbon--small");
        else _ribbonContainer.AddToClassList("ribbon--small");

        _layers.Clear();

        _root.AddManipulator(
            new TooltipManipulator(
                $"<size=12><uppercase><color=#9295E8>{wing.DisplayName}</color></uppercase></size>\n{wing.Description ?? "N/A"}"));

        foreach (var imageLayer in wing.config.imageLayers)
        {
            // Logger.LogDebug("Adding layer to ribbon " + imageLayer);
            var layer = new VisualElement();
            layer.AddToClassList("ribbon-layer");
            // if (!Settings.ShowAlwaysBigRibbons.Value) layer.AddToClassList("ribbon-layer__small");

            GameManager.Instance.Assets.LoadAssetAsync<Sprite>(imageLayer).Completed += handle =>
            {
                var sprite = handle.Result;
                if (sprite == null)
                {
                    Logger.LogError("Could not load sprite " + imageLayer);
                    return;
                }

                // Logger.LogInfo("OK: Successfully loaded sprite " + imageLayer);
                layer.style.backgroundImage = new StyleBackground(sprite);
            };

            _layers.Add(layer);
            _ribbonContainer.Add(layer);
        }
    }
}