using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace WayfarersWings.UI;

public class TooltipWindowController : MonoBehaviour
{
    private UIDocument _window;

    public static WindowOptions WindowOptions = new()
    {
        WindowId = "WayfarerWings_TooltipWindow",
        Parent = null,
        IsHidingEnabled = true,
        MoveOptions = new MoveOptions
        {
            IsMovingEnabled = false,
            CheckScreenBounds = false
        }
    };

    // The elements of the window that we need to access
    private VisualElement _root;
    private VisualElement _tooltip;
    private Label _tooltipText;

    public void OnEnable()
    {
        _window = GetComponent<UIDocument>();
        _root = _window.rootVisualElement[0];

        // Tooltip elements
        _tooltip = _root.Q<VisualElement>("tooltip");
        _tooltipText = _tooltip.Q<Label>("tooltip__text");
    }

    public void ToggleTooltip(bool isVisible, VisualElement target, string text = "")
    {
        if (isVisible)
        {
            _tooltipText.text = text;
            _tooltip.style.opacity = 1;
            _tooltip.style.left = -_root.worldBound.xMin + target.worldBound.xMin + target.worldBound.width / 2;
            _tooltip.style.top = -_root.worldBound.yMin + target.worldBound.yMin - 5;
        }
        else
        {
            _tooltip.style.opacity = 0;
        }
    }
}