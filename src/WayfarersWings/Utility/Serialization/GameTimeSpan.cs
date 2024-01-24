using WayfarersWings.Extensions;

namespace WayfarersWings.Utility.Serialization;

public struct GameTimeSpan
{
    private TimeSpan _timeSpan;
    public double Seconds { get; private set; }

    public GameTimeSpan(TimeSpan timeSpan)
    {
        _timeSpan = timeSpan;
        Seconds = timeSpan.ToGameSeconds();
    }
}