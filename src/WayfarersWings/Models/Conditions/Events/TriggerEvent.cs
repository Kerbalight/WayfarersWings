using System.Runtime.Serialization;
using KSP.Messages;
using Newtonsoft.Json;
using WayfarersWings.Utility;

namespace WayfarersWings.Models.Conditions.Events;

[Serializable]
public class TriggerEvent
{
    public string? eventName;
    public Type eventType = null!;

    [OnDeserialized]
    private void OnDeserialize(StreamingContext context)
    {
        if (eventName == null)
            throw new InvalidCastException("Event name must be specified");

        var eventFullName = eventName.StartsWith("KSP.Messages.") ? eventName : $"KSP.Messages.{eventName}";
        eventType = Type.GetType(eventFullName + ", " + Constants.GameMainAssemblyName) ??
                    throw new InvalidCastException($"Could not find event type '{eventFullName}'");
    }
}