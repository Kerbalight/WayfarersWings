using System.Collections;
using BepInEx.Logging;
using KSP.Game;
using KSP.Messages;
using KSP.Sim.impl;
using UnityEngine;

namespace WayfarersWings.Managers.Observer;

public class VesselsStateObserver : MonoBehaviour
{
    private static readonly ManualLogSource Logger =
        BepInEx.Logging.Logger.CreateLogSource("WayfarersWings.VesselsStateObserver");

    public static VesselsStateObserver Instance { get; set; } = null!;

    private readonly Dictionary<IGGuid, VesselObservedState> _vesselsState = new();

    public VesselObservedState? GetVesselObservedState(IGGuid? vesselId)
    {
        if (!vesselId.HasValue) return null;

        if (!_vesselsState.TryGetValue(vesselId.Value, out var state))
        {
            state = new VesselObservedState();
            _vesselsState.Add(vesselId.Value, state);
        }

        return state;
    }

    public void UpdateVessel(VesselComponent vessel)
    {
        var vesselId = vessel.GlobalId;
        if (!_vesselsState.TryGetValue(vesselId, out var state))
        {
            state = new VesselObservedState();
            _vesselsState.Add(vesselId, state);
            return;
        }

        var triggeredMessages = state.Update(vessel, out var hasChanged);
        if (!hasChanged) return;

        foreach (var message in triggeredMessages)
        {
            Logger.LogDebug($"[vessel={message.Vessel.Name}] Found difference, triggering message: " +
                            message.GetType().Name + ", message=" + message);
            MessageListener.Instance.MessageCenter.Publish(message.GetType(), message);
        }
    }

    private IEnumerator? _observeTask;

    public void Start()
    {
        Instance = this;

        MessageListener.Instance.Subscribe<GameStateChangedMessage>(OnGameStateChangedMessage);
        MessageListener.Instance.Subscribe<VesselChangedMessage>(OnVesselChangedMessage);
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

    private void OnVesselChangedMessage(MessageCenterMessage message)
    {
        var vesselChangedMessage = message as VesselChangedMessage;
        var vessel = vesselChangedMessage!.Vessel;
        Logger.LogInfo("Vessel changed to: " + vessel.Name);
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
}