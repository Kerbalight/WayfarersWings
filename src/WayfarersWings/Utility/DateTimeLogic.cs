using I2.Loc;
using KSP.UI.Binding;

namespace WayfarersWings.Utility;

public static class DateTimeLogic
{
    private static int _daysInYear;
    private static int _hoursInDay;
    private const string DateTimeFormat = "{0}Y {1}D {2:00}:{3:00}:{4:00}.{5:0.000}";
    private static string _dateTimeFormatLocalised = null!;
    private static bool _isInitialized;

    private static void Initialize()
    {
        _isInitialized = true;

        _daysInYear = PhysicsSettings.DaysInYear;
        _hoursInDay = PhysicsSettings.HoursInDay;
        if (LocalizationManager.TryGetTranslation(DateTimeFormat, out _dateTimeFormatLocalised))
            return;
        _dateTimeFormatLocalised = DateTimeFormat;
    }

    public static string FormatUniverseTime(double ut)
    {
        if (!_isInitialized) Initialize();

        var dateTime = UIValue_ReadNumber_DateTime.ComputeDateTime(ut, _hoursInDay, _daysInYear);
        return string.Format(_dateTimeFormatLocalised, dateTime.Years, dateTime.Days, (object)dateTime.Hours,
            (object)dateTime.Minutes, (object)dateTime.Seconds, (object)dateTime.Milliseconds);
    }
}