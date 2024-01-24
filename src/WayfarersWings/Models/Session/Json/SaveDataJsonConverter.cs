using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WayfarersWings.Extensions;

namespace WayfarersWings.Models.Session.Json;

/// <summary>
/// Actually not used, it's just a test to catch deserialization errors
/// </summary>
public class SaveDataJsonConverter : JsonConverter<SaveData>
{
    public override SaveData ReadJson(JsonReader reader, Type objectType, SaveData? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        SaveData target = new();
        try
        {
            JObject jObject = JObject.Load(reader);
            using JsonReader jObjectReader = JsonConvertExtension.CopyReaderForObject(reader, jObject);
            serializer.Populate(jObjectReader, target);
        }
        catch (Exception e)
        {
            WayfarersWingsPlugin.Instance.SWLogger.LogError(e);
            if (e is TargetInvocationException)
            {
                WayfarersWingsPlugin.Instance.SWLogger.LogError(
                    "Failed to load save data. Please report this to the mod author:" + e.InnerException);
            }
        }

        return target;
    }

    // Fall back to the default converter for the type
    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, SaveData? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}