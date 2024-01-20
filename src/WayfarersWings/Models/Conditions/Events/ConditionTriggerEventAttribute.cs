namespace WayfarersWings.Models.Conditions.Events;

using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ConditionTriggerEventAttribute : Attribute
{
    public Type TriggerEventType;

    public ConditionTriggerEventAttribute(Type triggerEventType)
    {
        TriggerEventType = triggerEventType;
    }
}