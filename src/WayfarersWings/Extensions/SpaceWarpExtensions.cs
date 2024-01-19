using KSP.Game.Flow;
using SpaceWarp.Patching.LoadingActions;
using WayfarersWings.Utility.Loading;

namespace WayfarersWings.Extensions;

public static class SpaceWarpExtensions
{
    public static void AddUIAddressablesLoadingAction<T>(string name, string label, Action<T> action)
        where T : UnityEngine.Object
    {
        SpaceWarp.API.Loading.Loading.AddGeneralLoadingAction((Func<FlowAction>)(() =>
            (FlowAction)new UIAddressableAction<T>(name, label, action)));
    }
}