namespace WayfarersWings.Models.Conditions.Events;

using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ConditionTriggerEventAttribute : Attribute
{
    public readonly Type TriggerEventType;

    public ConditionTriggerEventAttribute(Type triggerEventType)
    {
        TriggerEventType = triggerEventType;
    }
}