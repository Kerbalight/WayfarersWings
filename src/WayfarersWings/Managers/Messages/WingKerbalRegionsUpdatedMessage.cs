using WayfarersWings.Models.Session;

namespace WayfarersWings.Managers.Messages;

/// <summary>
/// Used in ConditionEventRegistry to process the condition when a KerbalProfile
/// discoverables/biomes are updated.
/// </summary>
public class WingKerbalRegionsUpdatedMessage : WingKerbalProfileUpdatedMessage
{
    public WingKerbalRegionsUpdatedMessage(KerbalProfile kerbalProfile) : base(kerbalProfile) { }
}