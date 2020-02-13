
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


    public delegate void MeshDisplayStateChangedEvent(MeshDisplayState meshDisplayState);

    public static event MeshDisplayStateChangedEvent OnMeshDisplayStateChanged;

    public static void MeshDisplayStateChanged(MeshDisplayState meshDisplayState) {
        OnMeshDisplayStateChanged?.Invoke(meshDisplayState);
    }
    
    public delegate void BoneDisplayStateChangedEvent(BoneDisplayState boneDisplayState);

    public static event BoneDisplayStateChangedEvent OnBoneDisplayStateChanged;

    public static void BoneDisplayStateChanged(BoneDisplayState boneDisplayState) {
        OnBoneDisplayStateChanged?.Invoke(boneDisplayState);
    }
    

    public delegate void PointLightDisplayStateChangedEvent(PointLightDisplayState pointLightDisplayState);

    public static event PointLightDisplayStateChangedEvent OnPointLightDisplayStateChanged;

    
    public static void PointLightDisplayStateChanged(PointLightDisplayState pointLightDisplayState) {
        OnPointLightDisplayStateChanged?.Invoke(pointLightDisplayState);
    }


    public delegate void ChangeManualPosingEvent(bool manualPosing);

    public static event ChangeManualPosingEvent OnChangeManualPosing;
    
    
    public static void ChangeManualPosing(bool manualPosing) {
        OnChangeManualPosing?.Invoke(manualPosing);
    }

    
    public delegate void ChangeLivePoseBlendshapesEvent(bool livePoseBlendshapes);

    public static event ChangeLivePoseBlendshapesEvent OnChangeLivePoseBlendshapes;

    
    public static void ChangeLivePoseBlendshapeRendering(bool livePoseBlendshapes) {
        OnChangeLivePoseBlendshapes?.Invoke(livePoseBlendshapes);
    }

    
    public delegate void ChangeLivePosesEvent(bool livePoses);

    public static event ChangeLivePosesEvent OnChangeLivePoses;

    public static void ChangeLivePoseRendering(bool livePoses) {
        OnChangeLivePoses?.Invoke(livePoses);
    }

    public delegate void ChangeLiveBodyShapeEvent(bool liveBodyShape);

    public static event ChangeLiveBodyShapeEvent OnChangeLiveBodyShape;

    
    public static void ChangeLiveBodyShapeRendering(bool liveBodyShape) {
        OnChangeLiveBodyShape?.Invoke(liveBodyShape);
    }
}
