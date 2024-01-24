using KSP.Game;
using KSP.Messages;
using WayfarersWings.Models.Session;

namespace WayfarersWings.Managers.Observer;

public class KerbalStateObserver
{
    // public void StartListeners()
    // {
    //     MessageListener.Instance.MessageCenter.PersistentSubscribe<FlagPlantedMessage>()
    // }

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
}