using System.Reflection;
using System.Runtime.Serialization;
using KSP.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine.Serialization;
using WayfarersWings.Models.Session.Json;

namespace WayfarersWings.Models.Session;

public class SaveData
{
    public string? SessionGuidString;

    public List<KerbalProfile> KerbalProfiles = [];

    /// Doesn't seem to work.
    [OnError]
    internal void OnError(StreamingContext context, ErrorContext errorContext)
    {
        if (errorContext.Error is TargetInvocationException)
        {
            WayfarersWingsPlugin.Instance.SWLogger.LogError(
                "Failed to load save data. Please report this to the mod author:" + errorContext.Error.InnerException);
        }

        errorContext.Handled = true;
    }
}