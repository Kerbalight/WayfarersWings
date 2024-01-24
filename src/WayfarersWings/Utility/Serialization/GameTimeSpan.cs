using WayfarersWings.Extensions;

namespace WayfarersWings.Utility.Serialization;

/// <summary>
/// Utility type for serializing time spans in a human-readable format,
/// compatible with KSP2 days and hours
/// </summary>
public struct GameTimeSpan
{
    private readonly double _days;
    private readonly double _hours;
    private readonly double _minutes;
    private readonly double _seconds;

    private double? _cachedSeconds;

    public double Seconds
    {
        get
        {
            if (_cachedSeconds.HasValue) return _cachedSeconds.Value;

            _cachedSeconds = _seconds +
                             _minutes * 60 +
                             _hours * PhysicsSettings.MinutesInHour * 60 +
                             _days * PhysicsSettings.HoursInDay * PhysicsSettings.MinutesInHour * 60;
            return _cachedSeconds.Value;
        }
    }


    public GameTimeSpan(double days = 0, double hours = 0, double minutes = 0, double seconds = 0)
    {
        _days = days;
        _hours = hours;
        _minutes = minutes;
        _seconds = seconds;
    }
}