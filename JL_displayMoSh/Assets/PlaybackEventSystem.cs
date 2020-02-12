
using System;
using JetBrains.Annotations;
using MoshPlayer.Scripts.BML.Display;
using UnityEngine;

public class PlaybackEventSystem : MonoBehaviour {

    public bool Paused = false;
    

    [PublicAPI]
    public void UpdateDisplaySpeed(float displaySpeed) {
        OnBroadcastDisplaySpeed?.Invoke(displaySpeed);
    }

    [PublicAPI]
    public void PausePlay() {
        Paused = !Paused;
        Debug.Log($"Paused: {Paused}");
        OnPauseToggleEvent?.Invoke(Paused);
    }

    public delegate void PauseToggleEvent(bool paused);

    public static event PauseToggleEvent OnPauseToggleEvent;

    public delegate void DisplaySpeedEvent(float displaySpeed);

    public static event DisplaySpeedEvent OnBroadcastDisplaySpeed;

    public delegate void FrameBroadCastEvent(float frame);

    public static event FrameBroadCastEvent OnFrameBroadcast;

    public static void BroadcastCurrentFrame(float frame) {
        OnFrameBroadcast?.Invoke(frame);
    }


    public delegate void BroadcastTotalFramesEvent(int totalFrames);

    public static event BroadcastTotalFramesEvent OnBroadcastTotalFrames;
    public static void BroadcastTotalFrames(int totalFrames) {
        OnBroadcastTotalFrames?.Invoke(totalFrames);
    }
    
    public delegate void UserFrameSelectEvent(float frame);

    public static event UserFrameSelectEvent OnUserFrameSelect;

    public static void UserSelectedFrame(float frame) {
        OnUserFrameSelect?.Invoke(frame);
    }


    public delegate void MeshDisplayStateChangedEvent(MeshDisplayState meshDisplayState);

    public static event MeshDisplayStateChangedEvent OnMeshDisplayStateChanged;

    public static void MeshDisplayStateChanged(MeshDisplayState meshDisplayState) {
        OnMeshDisplayStateChanged?.Invoke(meshDisplayState);
    }
}
