using BepInEx.Logging;
using SpaceWarp.API.SaveGameManager;
using WayfarersWings.Models.Session;
using WayfarersWings.UI;

namespace WayfarersWings.Managers;

public class SaveManager
{
    public static SaveManager Instance { get; private set; } = new();
    private readonly ManualLogSource _Logger = Logger.CreateLogSource("WayfarersWings.SaveManager");

    private SaveData? loadedSaveData;

    public void Register()
    {
        ModSaves.RegisterSaveLoadGameData<SaveData>(WayfarersWingsPlugin.ModGuid, SaveGameData, LoadGameData);
    }

    private void SaveGameData(SaveData dataToSave)
    {
        dataToSave.KerbalProfiles.Clear();
        foreach (var (kerbalId, profile) in WingsSessionManager.Instance.KerbalProfiles)
        {
            dataToSave.KerbalProfiles.Add(profile);
        }
    }

    private void LoadGameData(SaveData dataToLoad)
    {
        loadedSaveData = dataToLoad;
        _Logger.LogInfo("Loaded game data");
    }

    public void LoadGameDataInSession()
    {
        if (loadedSaveData == null)
        {
            _Logger.LogInfo("No save data loaded");
            return;
        }

        var loadedProfiles = new List<KerbalProfile>();
        foreach (var profile in loadedSaveData.KerbalProfiles)
        {
            profile.OnAfterGameLoad();
            loadedProfiles.Add(profile);
        }

        WingsSessionManager.Instance.Initialize(loadedProfiles);
        loadedSaveData = null;
        _Logger.LogInfo("Loaded game data into session");
    }
}