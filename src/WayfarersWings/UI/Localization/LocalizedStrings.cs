using I2.Loc;

namespace WayfarersWings.UI.Localization;

public static class LocalizedStrings
{
    public static LocalizedString AchievementsTitle = "WayfarersWings/UI/Achievements";

    // Ribbons
    public static LocalizedString FirstTime = "WayfarersWings/Ribbons/FirstTime";

    public static string GetTranslationWithParams(string localizationKey, Dictionary<string, string>? parameters)
    {
        var translation = LocalizationManager.GetTranslation(localizationKey);
        if (translation == null) return localizationKey;
        if (parameters == null) return translation;

        foreach (var (key, value) in parameters)
        {
            translation = translation.Replace($"{{{key}}}", value);
        }

        return translation;
    }
}