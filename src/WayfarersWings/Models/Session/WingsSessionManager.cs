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

    public Dictionary<IGGuid, KerbalProfile> KerbalProfiles { get; private set; } = [];

    private HashSet<string> _firstWingsAlreadyUnlocked = [];

    public void Initialize(IEnumerable<KerbalProfile> loadedProfiles)
    {
        KerbalProfiles = [];
        foreach (var profile in loadedProfiles)
        {
            KerbalProfiles.Add(profile.kerbalId, profile);
        }

        _firstWingsAlreadyUnlocked = new HashSet<string>();
        foreach (var (kerbalId, profile) in KerbalProfiles)
        {
            foreach (var entry in profile.Entries)
            {
                if (entry.Wing.config.isFirst) _firstWingsAlreadyUnlocked.Add(entry.Wing.config.name);
            }
        }
    }

    public bool IsFirstAlreadyUnlocked(Wing wing)
    {
        return wing.config.isFirst && _firstWingsAlreadyUnlocked.Contains(wing.config.name);
    }

    public KerbalProfile GetKerbalProfile(IGGuid kerbalId)
    {
        if (!KerbalProfiles.TryGetValue(kerbalId, out KerbalProfile kerbalWings))
        {
            kerbalWings = new KerbalProfile(kerbalId);
            KerbalProfiles.Add(kerbalId, kerbalWings);
        }

        return kerbalWings;
    }

    public void Award(Wing wing, KerbalInfo kerbalInfo)
    {
        var config = wing.config;
        if (IsFirstAlreadyUnlocked(wing)) return;

        var profile = GetKerbalProfile(kerbalInfo.Id);

        if (profile.HasWing(wing)) return;

        profile.AddWing(wing);
        if (wing.config.isFirst) _firstWingsAlreadyUnlocked.Add(wing.config.name);

        Logger.LogInfo("Awarded " + config.name + " to " + kerbalInfo.Attributes.GetFullName());
        MessageListener.Instance.MessageCenter.Publish(new WingAwardedMessage(kerbalInfo, wing));
    }
}