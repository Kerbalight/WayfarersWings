using BepInEx.Logging;
using KSP.Game;
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

    private List<KerbalWingEntries> _kerbalsWings = [];
    public IEnumerable<KerbalWingEntries> KerbalsWings => _kerbalsWings.AsReadOnly();

    private HashSet<string> _firstWingsAlreadyUnlocked = [];

    public void Initialize(IEnumerable<KerbalWingEntries> kerbalsWings)
    {
        _kerbalsWings = kerbalsWings.ToList();
        _firstWingsAlreadyUnlocked = new HashSet<string>();
        foreach (var kerbalWings in _kerbalsWings)
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

    public void Award(Wing wing, KerbalInfo kerbalInfo)
    {
        var config = wing.config;
        if (IsFirstAlreadyUnlocked(wing)) return;

        var kerbalWings = KerbalsWings.FirstOrDefault(kw => kw.KerbalId == kerbalInfo.Id);
        if (kerbalWings == null)
        {
            kerbalWings = new KerbalWingEntries(kerbalInfo.Id);
            _kerbalsWings.Add(kerbalWings);
        }

        if (kerbalWings.HasWing(wing)) return;

        kerbalWings.AddWing(wing);
        if (wing.config.isFirst) _firstWingsAlreadyUnlocked.Add(wing.config.name);

        Logger.LogInfo("Awarded " + config.name + " to " + kerbalInfo.Attributes.GetFullName());
        MessageListener.Instance.MessageCenter.Publish(new WingAwardedMessage(kerbalInfo, wing));
    }
}