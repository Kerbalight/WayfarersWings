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
            return new GameTimeSpan(TimeSpan.Zero);

        var tokens = timeSpanToken.Split(' ');
        var amount = double.Parse(tokens[0]);
        var timeSpan = tokens[1] switch
        {
            "weeks" => TimeSpan.FromDays(amount * 7),
            "days" => TimeSpan.FromDays(amount),
            "hours" => TimeSpan.FromHours(amount),
            "minutes" => TimeSpan.FromMinutes(amount),
            "seconds" => TimeSpan.FromSeconds(amount),
            _ => throw new NotImplementedException(
                "TimeSpan unit not implemented. Allowed only 'weeks', 'days', 'hours', 'minutes', 'seconds'")
        };
        return new GameTimeSpan(timeSpan);
    }
}