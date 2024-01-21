namespace WayfarersWings.Managers.Observer;

public class VesselObservedTimedProperty<T>
{
    public const int MaxHistory = 10;
    public readonly T[] History = new T[MaxHistory];

    public T Value
    {
        get => History[0];
        set
        {
            for (var i = MaxHistory - 1; i > 0; i--)
            {
                History[i] = History[i - 1];
            }

            History[0] = value;
        }
    }

    public void Reset()
    {
        for (var i = 0; i < MaxHistory; i++)
        {
            History[i] = default!;
        }
    }
}