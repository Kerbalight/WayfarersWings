using KSP.Sim.impl;

namespace WayfarersWings.Managers.Messages;

/// <summary>
/// Message sent when a vessel is recovered, after all Wayfarers Wings conditions are checked.
/// And mission wings are already awarded.
/// Furthermore every Kerbal is updated with the mission count, etc.
/// </summary>
public class WingMissionCompletedMessage : WingBaseMessage
{
    public VesselComponent? Vessel { get; set; }

    public WingMissionCompletedMessage(VesselComponent? vessel)
    {
        Vessel = vessel;
    }
}