using Newtonsoft.Json;
using WayfarersWings.Models.Conditions;
using WayfarersWings.Models.Conditions.Json;
using WayfarersWings.Models.Configs.Template;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace WayfarersWings.Models.Configs;

/// <summary>
/// Allows for the configuration of the wings dynamically, through JSON config
/// files in addressables.
/// </summary>
[Serializable]
public class WingsConfig
{
    public List<WingConfig> wings = [];
    public List<WingTemplateConfig> templates = [];

    private static JsonSerializerSettings _jsonSerializerSettings = new()
    {
        NullValueHandling = NullValueHandling.Include,
        Formatting = Formatting.Indented,
    };

    public static WingsConfig? Deserialize(string rawJson, out List<Exception> exceptions)
    {
        List<Exception> foundExceptions = [];
        var jsonSerializerSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented,

            Error = delegate(object sender, ErrorEventArgs args)
            {
                if (args.CurrentObject != args.ErrorContext.OriginalObject) return;

                foundExceptions.Add(args.ErrorContext.Error);
                args.ErrorContext.Handled = true;
            },
            Converters = new List<JsonConverter>()
            {
                new BaseConditionJsonConverter()
            }
        };

        var result = JsonConvert.DeserializeObject<WingsConfig>(rawJson, jsonSerializerSettings);
        exceptions = foundExceptions;
        return result;
    }
}