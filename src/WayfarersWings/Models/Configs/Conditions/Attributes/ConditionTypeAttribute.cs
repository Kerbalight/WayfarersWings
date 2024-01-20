namespace WayfarersWings.Models.Configs.Conditions.Attributes;

/// <summary>
/// An attribute that can be used to associate a condition type with a discriminator.
/// Can be used multiple times on the same class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ConditionTypeAttribute : Attribute
{
    public string Discriminator;
    public Type ConditionType;

    public ConditionTypeAttribute(string discriminator, Type conditionType)
    {
        Discriminator = discriminator;
        ConditionType = conditionType;
    }
}