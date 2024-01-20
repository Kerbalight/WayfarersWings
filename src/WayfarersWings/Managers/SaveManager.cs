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
        dataToSave.KerbalWings.Clear();
        foreach (var kerbalWings in WingsSessionManager.Instance.KerbalsWings)
        {
            dataToSave.KerbalWings.Add(new KerbalWingEntriesData(kerbalWings));
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

        var kerbalWings = new List<KerbalWingEntries>();
        foreach (var kerbalWingsData in loadedSaveData.KerbalWings)
        {
            kerbalWings.Add(new KerbalWingEntries(kerbalWingsData));
        }

        WingsSessionManager.Instance.Initialize(kerbalWings);
        loadedSaveData = null;
        _Logger.LogInfo("Loaded game data into session");
    }
}