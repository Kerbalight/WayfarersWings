using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WayfarersWings.Extensions;

public static class JsonConvertExtension
{
    /// <summary>
    /// Check if the token is a string or null, and if so, return the value.
    /// </summary>
    public static bool TryDeserializeStringOrNull(this JToken? token, out string? value)
    {
        if (token == null || token.Type == JTokenType.String || token.Type == JTokenType.Null)
        {
            value = token?.Value<string>();
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>
    /// Poor man's deep clone.
    /// </summary>
    public static T Clone<T>(this T source)
    {
        var serialized = JsonConvert.SerializeObject(source);
        return JsonConvert.DeserializeObject<T>(serialized)!;
    }

    public static JsonReader CopyReaderForObject(JsonReader reader, JToken jToken)
    {
        JsonReader jTokenReader = jToken.CreateReader();
        jTokenReader.Culture = reader.Culture;
        jTokenReader.DateFormatString = reader.DateFormatString;
        jTokenReader.DateParseHandling = reader.DateParseHandling;
        jTokenReader.DateTimeZoneHandling = reader.DateTimeZoneHandling;
        jTokenReader.FloatParseHandling = reader.FloatParseHandling;
        jTokenReader.MaxDepth = reader.MaxDepth;
        jTokenReader.SupportMultipleContent = reader.SupportMultipleContent;
        return jTokenReader;
    }
}