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

    public static KerbalRosterManager Roster => GameManager.Instance!.Game!.SessionManager!.KerbalRosterManager;

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

    /// <summary>
    /// Award a wing to all provided kerbals. This handles automatically
    /// the `AwardingInstant` so the first wings are unlocked to all kerbals
    /// in the same instant.
    /// </summary>
    public void AwardAll(Wing wing, IEnumerable<KerbalInfo> kerbals)
    {
        var instant = new AwardingInstant();
        foreach (var kerbal in kerbals)
        {
            Award(wing, kerbal, instant);
        }

        FinalizeInstant(instant);
    }

    /// <summary>
    /// Award a wing to a kerbal, only if it's not already unlocked.
    /// Furthermore, if the wing is a first wing, it will be marked as unlocked
    /// so no other kerbal will be able to unlock it.
    /// </summary>
    public void Award(Wing wing, KerbalInfo kerbalInfo, AwardingInstant? instant = null)
    {
        var config = wing.config;
        if (IsFirstAlreadyUnlocked(wing)) return;

        var profile = GetKerbalProfile(kerbalInfo.Id);

        if (profile.HasWing(wing))
        {
            Logger.LogDebug($"-> Kerbal {kerbalInfo.Attributes.GetFullName()} already has {config.name}");
            return;
        }

        profile.AddWing(wing);
        if (wing.config.isFirst)
        {
            // If we are in an instant, we need to add the first wing there so 
            // the `_firstWingsAlreadyUnlocked` is not modified.
            if (instant == null) _firstWingsAlreadyUnlocked.Add(wing.config.name);
            else instant.FirstWingsUnlocked.Add(wing.config.name);
        }

        Logger.LogInfo($"-> Awarded {config.name} to {kerbalInfo.Attributes.GetFullName()}");
        Core.Messages.Publish(new WingAwardedMessage(kerbalInfo, wing));
    }

    public void FinalizeInstant(AwardingInstant instant)
    {
        foreach (var wingCode in instant.FirstWingsUnlocked)
        {
            _firstWingsAlreadyUnlocked.Add(wingCode);
        }
    }

    public void Revoke(KerbalWingEntry entry, IGGuid kerbalId)
    {
        var profile = GetKerbalProfile(kerbalId);

        profile.RevokeWing(entry);
        if (entry.Wing.config.isFirst) _firstWingsAlreadyUnlocked.Remove(entry.Wing.config.name);

        Logger.LogInfo($"-> Revoked {entry.Wing.config.name} from {kerbalId}");
        Core.Messages.Publish(new WingRevokedMessage(kerbalId, entry.Wing));
    }

    /// <summary>
    /// Get all kerbals profiles based on the current roster.
    /// </summary>
    public List<KerbalProfile> GetAllKerbalsProfiles()
    {
        var kerbals = Roster.GetAllKerbals();
        var profiles = new List<KerbalProfile>();
        foreach (var kerbalInfo in kerbals)
        {
            profiles.Add(GetKerbalProfile(kerbalInfo.Id));
        }

        return profiles;
    }
}