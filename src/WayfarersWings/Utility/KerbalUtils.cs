using KSP.Game;
using KSP.Sim.impl;
using WayfarersWings.Models.Session;

namespace WayfarersWings.Utility;

public static class KerbalUtils
{
    /// <summary>
    /// KerbalInfo may be null when EVA is started (EVAEnteredMessage)
    /// </summary>
    /// <param name="kerbal"></param>
    /// <returns></returns>
    public static KerbalInfo? GetAndAssignIfNeededKerbalInfo(this KerbalComponent kerbal)
    {
        if (kerbal.KerbalInfo == null)
        {
            kerbal.AssignKerbalInfo();
        }

        return kerbal.KerbalInfo;
    }

    /// <summary>
    /// Get all kerbals in range around the player.
    /// </summary>
    public static IEnumerable<KerbalInfo> GetKerbalsInRange()
    {
        var kerbals = new List<KerbalInfo>();
        var vesselsInRange = GameManager.Instance.Game.ViewController.VesselsInRange;
        foreach (var vessel in vesselsInRange)
        {
            if (vessel.IsKerbalEVA) kerbals.Add(vessel.SimulationObject.Kerbal.KerbalInfo);
            else if (vessel.IsVesselAtRest() && vessel.LandedOrSplashed)
                kerbals.AddRange(WingsSessionManager.Roster.GetAllKerbalsInVessel(vessel.GlobalId));
        }

        return kerbals;
    }

    /// <summary>
    /// When an "EVA entered" message is dispatched, we are not sure if
    /// the _active_ vessel is the kerbal or it's the _pending_ vessel.
    /// </summary>
    public static VesselComponent? GetActiveOrPendingKerbalVessel()
    {
        var pendingVessel = GameManager.Instance.Game.ViewController.GetPendingActiveVessel();
        if (pendingVessel?.IsKerbalEVA == true) return pendingVessel;

        return GameManager.Instance.Game.ViewController.GetActiveSimVessel();
    }
}