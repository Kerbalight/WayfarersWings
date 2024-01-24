namespace WayfarersWings.Extensions;

public static class TimeSpanExtensions
{
    public static double ToGameSeconds(this TimeSpan timeSpan)
    {
        return timeSpan.Seconds +
               timeSpan.Minutes * 60 +
               timeSpan.Hours * PhysicsSettings.MinutesInHour * 60 +
               timeSpan.Days * PhysicsSettings.HoursInDay * PhysicsSettings.MinutesInHour * 60;
    }
}