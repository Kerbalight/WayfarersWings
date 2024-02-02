using I2.Loc;
using JetBrains.Annotations;

namespace WayfarersWings.UI.Localization;

public static class LocalizedStrings
{
    public static LocalizedString AchievementsTitle = "WayfarersWings/UI/Achievements";
    public static LocalizedString MissionAchievementsTitle = "WayfarersWings/UI/MissionAchievements";
    public static LocalizedString TotalMissionTime = "WayfarersWings/UI/TotalMissionTime";
    public static LocalizedString TotalEvaTime = "WayfarersWings/UI/TotalEvaTime";
    public static LocalizedString MissionsCompleted = "WayfarersWings/UI/MissionsCompleted";
    public static LocalizedString Status = "WayfarersWings/UI/Status";
    public static LocalizedString OnMission = "WayfarersWings/UI/OnMission";
    public static LocalizedString ActiveVessel = "WayfarersWings/UI/ActiveVessel";
    public static LocalizedString Available = "WayfarersWings/UI/Available";
    public static LocalizedString Starred = "WayfarersWings/UI/Starred";
    public static LocalizedString All = "WayfarersWings/UI/All";
    public static LocalizedString Dead = "WayfarersWings/UI/Dead";
    public static LocalizedString AwardWingTitle = "WayfarersWings/UI/AwardWingTitle";
    public static LocalizedString AwardToKerbal = "WayfarersWings/UI/AwardToKerbal";
    public static LocalizedString ConfirmRevokeButton = "WayfarersWings/UI/ConfirmRevokeButton";
    public static LocalizedString AchievementsTabRibbons = "WayfarersWings/UI/Ribbons";
    public static LocalizedString AchievementsTabKerbals = "WayfarersWings/UI/Kerbals";


    public const string KerbalMissionsCount = "WayfarersWings/UI/KerbalMissionsCount";

    // Sorting
    public static LocalizedString SortByName = "WayfarersWings/UI/SortByName";
    public static LocalizedString SortByPoints = "WayfarersWings/UI/SortByPoints";

    // Ribbons
    public const string FirstTimeName = "WayfarersWings/Ribbons/FirstTimeName";
    public const string FirstTime = "WayfarersWings/Ribbons/FirstTime";

    public static string GetTranslationWithParams(string localizationKey, Dictionary<string, string>? parameters)
    {
        var translation = LocalizationManager.GetTranslation(localizationKey);
        if (translation == null) return localizationKey;
        if (parameters == null) return translation;

        foreach (var (key, value) in parameters)
        {
            // Allows substitution of other localization keys
            var substitution = value?.StartsWith("#") == true
                ? LocalizationManager.GetTranslation(value[1..]) ?? value
                : value;

            translation = translation.Replace($"{{{key}}}", substitution);
        }

        return translation;
    }
}