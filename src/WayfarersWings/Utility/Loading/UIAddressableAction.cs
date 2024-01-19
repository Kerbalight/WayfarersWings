using KSP.Game;
using KSP.Game.Flow;
using SpaceWarp.Patching.LoadingActions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.UIElements;

namespace WayfarersWings.Utility.Loading;

public class UIAddressableAction<T> : FlowAction where T : UnityEngine.Object
{
    private string _label;
    private Action<T> _action;

    public UIAddressableAction(string name, string label, Action<T> action) : base(name)
    {
        this._label = label;
        this._action = action;
    }

    private bool DoesLabelExist(object label)
    {
        return GameManager.Instance.Assets._registeredResourceLocators.Any<IResourceLocator>(
                   (Func<IResourceLocator, bool>)(locator => locator.Keys.Contains<object>(label))) ||
               Addressables.ResourceLocators.Any<IResourceLocator>(
                   (Func<IResourceLocator, bool>)(locator => locator.Keys.Contains<object>(label)));
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        if (!DoesLabelExist((object)this._label))
        {
            Debug.Log((object)("[UIAddressableAction] Skipping loading addressables for " + this._label +
                               " which does not exist."));
            resolve();
        }
        else
        {
            try
            {
                GameManager.Instance.Assets.LoadByLabel<T>(this._label, this._action,
                    (Action<IList<T>>)(assetLocations =>
                    {
                        // if (assetLocations != null)
                        //     Addressables.Release<IList<T>>(assetLocations);
                        resolve();
                    }));
            }
            catch (Exception ex)
            {
                Debug.LogError((object)ex.ToString());
                reject((string)null);
            }
        }
    }
}