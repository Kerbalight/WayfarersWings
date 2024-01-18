using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace WayfarersWings.Models.Conditions.Events;

[Serializable]
public class TriggerEvent
{
    public string eventName;
    public Type eventType;

    [OnDeserialized]
    private void OnDeserialize()
    {
        var eventFullName = eventName.StartsWith("KSP.Messages.") ? eventName : $"KSP.Messages.{eventName}";
        eventType = Type.GetType(eventFullName) ??
                    throw new InvalidOperationException($"Could not find event type '{eventFullName}'");
    }
}