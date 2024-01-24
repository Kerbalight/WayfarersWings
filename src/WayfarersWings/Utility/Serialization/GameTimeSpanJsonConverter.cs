using Newtonsoft.Json;

namespace WayfarersWings.Utility.Serialization;

public class GameTimeSpanJsonConverter : JsonConverter<GameTimeSpan>
{
    public override void WriteJson(JsonWriter writer, GameTimeSpan value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override bool CanWrite => false;

    public override GameTimeSpan ReadJson(JsonReader reader, Type objectType, GameTimeSpan existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var timeSpanToken = (string?)reader.Value;
        if (string.IsNullOrEmpty(timeSpanToken))
            return new GameTimeSpan();

        var tokens = timeSpanToken.Split(' ');
        var amount = double.Parse(tokens[0]);
        var timeSpan = tokens[1] switch
        {
            "weeks" => new GameTimeSpan(days: amount * 7),
            "days" => new GameTimeSpan(days: amount),
            "hours" => new GameTimeSpan(hours: amount),
            "minutes" => new GameTimeSpan(minutes: amount),
            "seconds" => new GameTimeSpan(seconds: amount),
            _ => throw new NotImplementedException(
                "GameTimeSpan unit not implemented. Allowed only 'weeks', 'days', 'hours', 'minutes', 'seconds'")
        };
        return timeSpan;
    }
}