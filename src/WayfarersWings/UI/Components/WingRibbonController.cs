using BepInEx.Logging;
using KSP.Game;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.UI.Components;

public class WingRibbonController
{
    private static ManualLogSource
        Logger = BepInEx.Logging.Logger.CreateLogSource("WayfarerWings.WingRibbonController");

    private VisualElement _root;
    private VisualElement _ribbonContainer;
    private VisualElement _base;
    private List<VisualElement> _layers;

    public WingRibbonController(VisualElement root)
    {
        _root = root;
        _ribbonContainer = _root.Q<VisualElement>("ribbon-container");
        _base = _root.Q<VisualElement>("base");
        _layers = new List<VisualElement>();
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
            _root.Remove(layer);
        }

        _layers.Clear();

        foreach (var imageLayer in wing.Config.imageLayers)
        {
            Logger.LogDebug("Adding layer to ribbon " + imageLayer);
            var layer = new VisualElement();
            layer.AddToClassList("ribbon-layer");

            GameManager.Instance.Assets.LoadAssetAsync<Sprite>(imageLayer).Completed += handle =>
            {
                var sprite = handle.Result;
                if (sprite == null)
                {
                    Logger.LogError("Could not load sprite " + imageLayer);
                    return;
                }

                Logger.LogInfo("OK: Successfully loaded sprite " + imageLayer);
                layer.style.backgroundImage = new StyleBackground(sprite);
            };

            _layers.Add(layer);
            _ribbonContainer.Add(layer);
        }
    }
}