namespace WayfarersWings.Models.Session.Json;

public interface IJsonSaved
{
    public void OnAfterGameLoad();
    public void OnBeforeGameSave();
}