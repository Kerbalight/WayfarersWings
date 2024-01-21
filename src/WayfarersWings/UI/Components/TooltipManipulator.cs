using UnityEngine.UIElements;

namespace WayfarersWings.UI.Components;

public class TooltipManipulator : MouseManipulator
{
    private string _tooltipText;

    public TooltipManipulator(string tooltipText)
    {
        _tooltipText = tooltipText;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseEnterEvent>(OnMouseIn);
        target.RegisterCallback<MouseLeaveEvent>(OnMouseOut);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseEnterEvent>(OnMouseIn);
        target.UnregisterCallback<MouseLeaveEvent>(OnMouseOut);
    }

    private void OnMouseIn(MouseEnterEvent evt)
    {
        MainUIManager.Instance.AppWindow.ToggleTooltip(true, target, _tooltipText);
    }

    private void OnMouseOut(MouseLeaveEvent evt)
    {
        MainUIManager.Instance.AppWindow.ToggleTooltip(false, target);
    }
}