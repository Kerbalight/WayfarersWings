namespace WayfarersWings.Managers.Observer.Properties.Types;

public abstract class VesselObservedDiscreteProperty : VesselObservedProperty<int>
{
    /// <summary>
    /// A discrete property is considered changed when it crosses a discrete point.
    /// This is useful to reduce spamming of messages.
    /// </summary>
    public override bool HasChanged
    {
        get
        {
            // Early out if the value hasn't changed
            if (PreviousValue == Value)
                return false;

            // Early out if there are no discrete points
            if (!VesselsStateObserver.Instance.PropertiesDiscretePoints.ContainsKey(GetType()))
                return true;

            foreach (var discretePoint in VesselsStateObserver.Instance.PropertiesDiscretePoints[GetType()])
            {
                if (Value > discretePoint && PreviousValue <= discretePoint
                    || Value < discretePoint && PreviousValue >= discretePoint)
                {
                    return true;
                }
            }

            return false;
        }
    }
}