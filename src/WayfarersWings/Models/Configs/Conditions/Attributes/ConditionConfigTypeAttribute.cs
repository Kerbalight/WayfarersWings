namespace WayfarersWings.Models.Configs.Conditions.Attributes;

public class ConditionConfigTypeAttribute : Attribute
{
    public string Discriminator;
    public Type ConfigType;

    public ConditionConfigTypeAttribute(string discriminator, Type configType)
    {
        Discriminator = discriminator;
        ConfigType = configType;
    }
}