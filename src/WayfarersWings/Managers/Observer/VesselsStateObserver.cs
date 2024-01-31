using System.Collections;
using BepInEx.Logging;
using KSP.Game;
using KSP.Messages;
using KSP.Sim.impl;
using UnityEngine;
using WayfarersWings.Managers.Observer.Properties.Types;

namespace WayfarersWings.Managers.Observer;

public class VesselsStateObserver : MonoBehaviour
{
    private static readonly ManualLogSource Logger =
        BepInEx.Logging.Logger.CreateLogSource("WayfarersWings.VesselsStateObserver");

    public static VesselsStateObserver Instance { get; set; } = null!;

    /// <summary>
    /// Keep track of discrete quantization of specific vessel properties.
    /// Wing conditions requires theses properties to be quantized, e.g. > 10,
    /// then each time the property is updated, we need to check if the condition
    /// "crossed" the threshold.
    /// Only if it crossed the threshold, we trigger the condition.
    /// </summary>
    public Dictionary<Type, List<int>> PropertiesDiscretePoints = new();

    /// <summary>
    /// If the vessel changed, this is the previous vessel.
    /// </summary>
    public VesselComponent? PreviousVessel { get; private set; }

    private readonly Dictionary<IGGuid, VesselObservedState> _vesselsState = new();

    public VesselObservedState? GetVesselObservedState(IGGuid? vesselId)
    {
        if (!vesselId.HasValue) return null;
        return GetVesselObservedState(vesselId.Value);
    }

    public VesselObservedState GetVesselObservedState(IGGuid vesselId)
    {
        if (!_vesselsState.TryGetValue(vesselId, out var state))
        {
            state = new VesselObservedState();
            _vesselsState.Add(vesselId, state);
        }

        return state;
    }

    public VesselObservedState GetVesselObservedState(VesselComponent vessel)
    {
        return GetVesselObservedState(vessel.GlobalId);
    }

    /// <summary>
    /// When a vessel is first observed, we need to initialize its observed state.
    /// </summary>
    private void StartObservingVessel(VesselComponent vessel)
    {
        var state = GetVesselObservedState(vessel);
        state.Start(vessel);
    }

    private void UpdateVessel(VesselComponent vessel)
    {
        var vesselId = vessel.GlobalId;
        var state = GetVesselObservedState(vesselId);

        var triggeredMessages = state.Update(vessel, out var hasChanged);
        if (!hasChanged) return;

        foreach (var message in triggeredMessages)
        {
            Logger.LogDebug($"[vessel={message.Vessel.Name}] Found difference, triggering message: " +
                            message.GetType().Name);
            Core.Messages.Publish(message.GetType(), message);
        }
    }

    private IEnumerator? _observeTask;

    public void Start()
    {
        Instance = this;

        MessageListener.Subscribe<GameStateChangedMessage>(OnGameStateChangedMessage);
        MessageListener.Subscribe<VesselChangingMessage>(OnVesselChangingMessage);
        MessageListener.Subscribe<VesselChangedMessage>(OnVesselChangedMessage);
    }

    private void OnGameStateChangedMessage(MessageCenterMessage message)
    {
        var gameStateMessage = message as GameStateChangedMessage;
        var isValid = gameStateMessage!.CurrentState is GameState.FlightView or GameState.Map3DView;

        switch (isValid)
        {
            case true when _observeTask == null:
                Logger.LogInfo("Starting vessel observe task");
                _observeTask = RunObserveTask();
                StartCoroutine(_observeTask);
                break;
            case false when _observeTask != null:
                Logger.LogInfo("Stopping vessel observe task");
                StopCoroutine(_observeTask);
                _observeTask = null;
                break;
        }
    }

    /// <summary>
    /// We need to keep track of the previous vessel, since when `VesselChangedMessage` is triggered,
    /// the new vessel is already active and we can't get the previous one.
    /// </summary>
    private void OnVesselChangingMessage(MessageCenterMessage message)
    {
        var vesselChangingMessage = message as VesselChangingMessage;
        PreviousVessel = vesselChangingMessage?.OldVessel;
    }

    private void OnVesselChangedMessage(MessageCenterMessage message)
    {
        var vesselChangedMessage = message as VesselChangedMessage;
        var vessel = vesselChangedMessage?.Vessel;
        if (vessel == null) return;
        Logger.LogInfo("Vessel changed to: " + vessel.Name);
        StartObservingVessel(vessel);
        UpdateVessel(vessel);
    }

    private IEnumerator RunObserveTask()
    {
        while (true)
        {
            if (GameManager.Instance.Game.ViewController.TryGetActiveSimVessel(out var vessel))
            {
                UpdateVessel(vessel);
            }

            yield return new WaitForSeconds(1.0f);
        }
        // ReSharper disable once IteratorNeverReturns
    }

    #region Discrete quantization

    // I really don't like this, but I can't think of a smarter way to do it.
    public void ObservePropertyAtPoint<T>(int point) where T : VesselObservedDiscreteProperty
    {
        var type = typeof(T);
        if (!PropertiesDiscretePoints.ContainsKey(type)) PropertiesDiscretePoints[type] = [];
        PropertiesDiscretePoints[type].Add(point);
    }

    #endregion
}