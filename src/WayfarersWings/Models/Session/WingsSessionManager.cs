using BepInEx.Logging;
using KSP.Game;
using KSP.Sim.impl;
using WayfarersWings.Managers;
using WayfarersWings.Managers.Messages;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Session;

public class WingsSessionManager
{
    private static readonly ManualLogSource Logger =
        BepInEx.Logging.Logger.CreateLogSource("WayfarersWings.SessionManager");

    public static WingsSessionManager Instance { get; private set; } = new();

    public string SessionGuidString;

    public static KerbalRosterManager Roster => GameManager.Instance?.Game?.SessionManager?.KerbalRosterManager;

    public Dictionary<IGGuid, KerbalWingEntries> KerbalsWings { get; private set; } = [];

    private HashSet<string> _firstWingsAlreadyUnlocked = [];

    public void Initialize(IEnumerable<KerbalWingEntries> kerbalsWings)
    {
        KerbalsWings = [];
        foreach (var kerbalWings in kerbalsWings)
        {
            KerbalsWings.Add(kerbalWings.KerbalId, kerbalWings);
        }

        _firstWingsAlreadyUnlocked = new HashSet<string>();
        foreach (var (kerbalId, kerbalWings) in KerbalsWings)
        {
            foreach (var entry in kerbalWings.Entries)
            {
                if (entry.Wing.config.isFirst) _firstWingsAlreadyUnlocked.Add(entry.Wing.config.name);
            }
        }
    }

    public bool IsFirstAlreadyUnlocked(Wing wing)
    {
        return wing.config.isFirst && _firstWingsAlreadyUnlocked.Contains(wing.config.name);
    }

    public KerbalWingEntries GetKerbalWings(IGGuid kerbalId)
    {
        if (!KerbalsWings.TryGetValue(kerbalId, out KerbalWingEntries kerbalWings))
        {
            kerbalWings = new KerbalWingEntries(kerbalId);
            KerbalsWings.Add(kerbalId, kerbalWings);
        }

        return kerbalWings;
    }

    public void Award(Wing wing, KerbalInfo kerbalInfo)
    {
        var config = wing.config;
        if (IsFirstAlreadyUnlocked(wing)) return;

        var kerbalWings = GetKerbalWings(kerbalInfo.Id);

        if (kerbalWings.HasWing(wing)) return;

        kerbalWings.AddWing(wing);
        if (wing.config.isFirst) _firstWingsAlreadyUnlocked.Add(wing.config.name);

        Logger.LogInfo("Awarded " + config.name + " to " + kerbalInfo.Attributes.GetFullName());
        MessageListener.Instance.MessageCenter.Publish(new WingAwardedMessage(kerbalInfo, wing));
    }
}