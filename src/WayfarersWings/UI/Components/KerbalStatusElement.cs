using UnityEngine.UIElements;
using WayfarersWings.Models.Session;
using WayfarersWings.UI.Localization;

namespace WayfarersWings.UI.Components;

public static class KerbalStatusElement
{
    public static void SetStatus(VisualElement statusIcon, KerbalProfile profile)
    {
        statusIcon.RemoveFromClassList("kerbal-status--default");
        statusIcon.RemoveFromClassList("kerbal-status--assigned");
        statusIcon.RemoveFromClassList("kerbal-status--available");
        statusIcon.RemoveFromClassList("kerbal-status--assigned-active");

        switch (profile.GetStatus())
        {
            case KerbalStatus.Assigned:
                statusIcon.AddToClassList("kerbal-status--assigned");
                break;
            case KerbalStatus.AssignedActive:
                statusIcon.AddToClassList("kerbal-status--assigned-active");
                break;
            case KerbalStatus.Available:
                statusIcon.AddToClassList("kerbal-status--available");
                break;
            case KerbalStatus.Unknown:
            case KerbalStatus.Dead:
            default:
                statusIcon.AddToClassList("kerbal-status--default");
                break;
        }
    }

    public static void SetText(Label label, KerbalProfile profile)
    {
        label.text = profile.GetStatus() switch
        {
            KerbalStatus.Assigned => $"<color=#ff685c>{LocalizedStrings.OnMission}</color>",
            KerbalStatus.AssignedActive => $"<color=#696DFF>{LocalizedStrings.ActiveVessel}</color>",
            KerbalStatus.Available => $"<color=#00FF66>{LocalizedStrings.Available}</color>",
            _ => LocalizedStrings.Dead
        };
    }
}