using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MoshPlayer.Scripts.Display;
using MoshPlayer.Scripts.InGameUI;
using MoshPlayer.Scripts.SMPLModel;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.Playback {
    public class PlaybackEventSystem : MonoBehaviour {

        public bool Paused = false;
    
        [FormerlySerializedAs("nextKeys")]
        [SerializeField]
        List<KeyCode> NextKeys = new List<KeyCode>();

        [SerializeField]
        KeyCode PrevKey = default;
        
        [FormerlySerializedAs("quitKeys")]
        [SerializeField]
        List<KeyCode> QuitKeys = new List<KeyCode>();

        [SerializeField] KeyCode ToggleAllUIKey = KeyCode.U;

        [SerializeField] List<GameObject> ui = default;

        bool uiOn = true;
        bool allowKeyboardControls;

        void OnEnable() {
            KeyboardControlEvents.OnDisableKeyboardControls += DisableKeyboardControls;
            KeyboardControlEvents.OnEnableKeyboardControls += EnableKeyboardControls;
        }
        
        void OnDisable() {
            KeyboardControlEvents.OnDisableKeyboardControls -= DisableKeyboardControls;
            KeyboardControlEvents.OnEnableKeyboardControls -= EnableKeyboardControls;
        }

        void EnableKeyboardControls() {
            allowKeyboardControls = true;
        }

        void DisableKeyboardControls() {
            allowKeyboardControls = false;
        }


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

        public void Update() {
            if (allowKeyboardControls) {
                ListenForKeyboardPresses();
            }
        }

        void ListenForKeyboardPresses() {

            if (Input.GetKeyDown(ToggleAllUIKey)) {
                ToggleAllUI();
            }
            
            foreach (KeyCode key in NextKeys) {
                if (Input.GetKeyDown(key)) {
                    GoToNextAnimation();
                }
            }

            if (Input.GetKeyDown(PrevKey)) {
                Debug.Log("Delete");
                GoToPreviousAnimation();
            }

            foreach (KeyCode key in QuitKeys) {
                if (Input.GetKeyDown(key)) {
                    Quit();
                }
            }
        }

        void ToggleAllUI() {
            uiOn = !uiOn;
            foreach (GameObject uiObject in ui) {
                uiObject.SetActive(!uiObject.activeSelf);
            }
            
        }

        static void Quit() {
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
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


        public delegate void NextAnimationEvent();

        public static event NextAnimationEvent OnNextAnimation;

        public static void GoToNextAnimation() {
            StopPlayingAllAnimations();
            OnNextAnimation?.Invoke();
        }

        public delegate void PreviousAnimationEvent( );

        public static event PreviousAnimationEvent OnPreviousAnimation;

        public static void GoToPreviousAnimation( ) {
            OnPreviousAnimation?.Invoke();
        }

        public delegate void RestartAnimationsEvent();

        public static event RestartAnimationsEvent OnRestartAnimations;
        
        public static void RestartAnimations() {
            OnRestartAnimations?.Invoke();
        }

        public delegate void PlayerProgressTextEvent(string text);

        public static event PlayerProgressTextEvent OnUpdatePlayerProgress;

        public static void UpdatePlayerProgress(string text) {
            OnUpdatePlayerProgress?.Invoke(text);
        }


        public delegate void LoadAnimationsEvent(string listFile, string animationsFolder);

        public static event LoadAnimationsEvent OnLoadAnimations;

        public static void LoadAnimations(string listFile, string animationsFolder) {
            OnLoadAnimations?.Invoke(listFile, animationsFolder);
        }

        public delegate void LoadSingleAnimationEvent(string singleFile);

        public static event LoadSingleAnimationEvent OnLoadSingleAnimation;

        public static void LoadSingleAnimation(string singleFile) {
            OnLoadSingleAnimation?.Invoke(singleFile);
        }

        public delegate void DoneLoadingAnimationsEvent();

        public static event DoneLoadingAnimationsEvent OnDoneLoadingAnimations;

        public static void AnimationsDoneLoading( ){
            OnDoneLoadingAnimations?.Invoke();
        }

        public delegate void StopAllAnimationsEvent();

        public static event StopAllAnimationsEvent OnStopAllAnimations;

        public static void StopPlayingAllAnimations() {
            OnStopAllAnimations?.Invoke();
        }


        public delegate void PlayingNewAnimationSetEvent(List<MoshAnimation> animations);

        public static event PlayingNewAnimationSetEvent OnPlayingNewAnimationSet;

        public static void PlayingNewAnimationSet(List<MoshAnimation> animations) {
            OnPlayingNewAnimationSet?.Invoke(animations);
        }


        public delegate void ChangeSnapToGroundEvent(GroundSnapType snapType);

        public static event ChangeSnapToGroundEvent OnChangeSnapToGround;

        public static void ChangeSnapToGround(GroundSnapType snapType) {
            OnChangeSnapToGround?.Invoke(snapType);
        }

        public delegate void ChangeUpdateYTranslationEvent(bool updateYTranslation);

        public static event ChangeUpdateYTranslationEvent OnChangeUpdateYTranslation;

        public static void ChangeUpdateYTranslation(bool updateYTranslation) {
            OnChangeUpdateYTranslation?.Invoke(updateYTranslation);
        }

        public delegate void ChangeUpdateXzTranslationEvent(bool changeUpdateXzTranslation);

        public static event ChangeUpdateXzTranslationEvent OnChangeUpdateXzTranslation;

        public static void ChangeUpdateXzTranslation(bool changeUpdateXzTranslation) {
            OnChangeUpdateXzTranslation?.Invoke(changeUpdateXzTranslation);
        }

        public delegate void ChangeIndividualizedBodyEvent(bool showIndividualizedBody);

        public static event ChangeIndividualizedBodyEvent OnChangeIndividualizedBody;

        public static void ChangeIndividualizedBody(bool showIndividualizedBody) {
            OnChangeIndividualizedBody?.Invoke(showIndividualizedBody);
        }

        public delegate void LoadNewAnimationsEvent();

        public static event LoadNewAnimationsEvent OnLoadNewAnimations;

        public static void LoadNewAnimations() {
            OnLoadNewAnimations?.Invoke();
        }

        public delegate void ChangeLoopStateEvent(bool state);

        public static event ChangeLoopStateEvent OnChangeLoopState;

        public static void ChangeLoopState(bool state) {
            OnChangeLoopState?.Invoke(state);
        }

       
    }
}
