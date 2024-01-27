using KSP.Game;
using KSP.Sim.impl;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Managers.Messages;

public class WingRevokedMessage : WingBaseMessage
{
    public IGGuid KerbalId { get; set; }
    public Wing RevokedWing { get; set; }

    public WingRevokedMessage(IGGuid kerbalId, Wing revokedWing)
    {
        KerbalId = kerbalId;
        RevokedWing = revokedWing;
    }
}