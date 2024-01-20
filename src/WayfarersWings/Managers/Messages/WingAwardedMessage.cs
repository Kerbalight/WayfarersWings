using KSP.Game;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Managers.Messages;

public class WingAwardedMessage : WingBaseMessage
{
    public KerbalInfo KerbalInfo { get; set; }
    public Wing AwardedWing { get; set; }

    public WingAwardedMessage(KerbalInfo kerbalInfo, Wing awardedWing)
    {
        KerbalInfo = kerbalInfo;
        AwardedWing = awardedWing;
    }
}