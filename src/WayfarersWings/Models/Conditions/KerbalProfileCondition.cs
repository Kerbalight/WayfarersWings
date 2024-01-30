using KSP.Messages;
using Newtonsoft.Json;
using WayfarersWings.Models.Conditions.Events;
using WayfarersWings.Models.Wings;
using WayfarersWings.Utility.Serialization;

namespace WayfarersWings.Models.Conditions;

[Serializable]
[ConditionTriggerEvent(typeof(VesselRecoveredMessage))]
[ConditionTriggerEvent(typeof(EVALeftMessage))]
public class KerbalProfileCondition : BaseCondition
{
    /// <summary>
    /// Minimum number of missions completed by the Kerbal
    /// </summary>
    public int? minMissionCount;

    /// <summary>
    /// Minimum time (TimeSpan like "15 days") the Kerbal has spent on a single mission
    /// </summary>
    [JsonConverter(typeof(GameTimeSpanJsonConverter))]
    public GameTimeSpan? minMissionTime;

    /// <summary>
    /// Minimum time (TimeSpan like "10 days") the Kerbal has spent on all missions
    /// </summary>
    [JsonConverter(typeof(GameTimeSpanJsonConverter))]
    public GameTimeSpan? minTotalMissionTime;

    /// <summary>
    /// Minimum time (like "30 minutes") the Kerbal has spent on a single EVA in space
    /// </summary>
    [JsonConverter(typeof(GameTimeSpanJsonConverter))]
    public GameTimeSpan? minEvaSpaceTime;

    /// <summary>
    /// Minimum time (TimeSpan like "30 seconds") the Kerbal has spent on a single EVA (atmosphere or space)
    /// </summary>
    [JsonConverter(typeof(GameTimeSpanJsonConverter))]
    public GameTimeSpan? minEvaTime;

    /// <summary>
    /// Minimum time (TimeSpan like "10 days") the Kerbal has spent on all EVAs
    /// </summary>
    [JsonConverter(typeof(GameTimeSpanJsonConverter))]
    public GameTimeSpan? minTotalEvaTime;

    /// <summary>
    /// Minimum time (TimeSpan like "10 days") the Kerbal has spent on all EVAs in space
    /// </summary>
    [JsonConverter(typeof(GameTimeSpanJsonConverter))]
    public GameTimeSpan? minTotalEvaSpaceTime;

    public override bool IsValid(Transaction transaction)
    {
        if (transaction.KerbalProfile is not { } profile) return false;

        if (minMissionCount.HasValue && !(profile.missionsCount >= minMissionCount))
            return false;
        if (minMissionTime.HasValue && !(profile.lastMissionTime >= minMissionTime.Value.Seconds))
            return false;
        if (minTotalMissionTime.HasValue && !(profile.totalMissionTime >= minTotalMissionTime.Value.Seconds))
            return false;

        if (minEvaTime.HasValue && !(profile.lastEvaTime >= minEvaTime.Value.Seconds))
            return false;
        if (minEvaSpaceTime.HasValue && !(profile.lastEvaSpaceTime >= minEvaSpaceTime.Value.Seconds))
            return false;
        if (minTotalEvaTime.HasValue && !(profile.TotalEvaTime >= minTotalEvaTime.Value.Seconds))
            return false;
        if (minTotalEvaSpaceTime.HasValue && !(profile.totalEvaSpaceTime >= minTotalEvaSpaceTime.Value.Seconds))
            return false;

        return true;
    }
}