using WayfarersWings.Models.Session;

namespace WayfarersWings.Managers.Messages;

public class WingKerbalProfileUpdatedMessage : WingBaseMessage
{
    public WingKerbalProfileUpdatedMessage(KerbalProfile kerbalProfile)
    {
        KerbalProfile = kerbalProfile;
    }

    public KerbalProfile KerbalProfile { get; set; }
}