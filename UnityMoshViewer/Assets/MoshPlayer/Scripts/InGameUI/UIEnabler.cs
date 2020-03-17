﻿using MoshPlayer.Scripts.Playback;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.InGameUI {
    public class UIEnabler : MonoBehaviour {

        [FormerlySerializedAs("PlaybackCanvas")] 
        [SerializeField]
        GameObject playbackCanvas = default;

        [FormerlySerializedAs("AnimationControls")] 
        [SerializeField]
        GameObject animationControls = default;
        
        [FormerlySerializedAs("CameraControls")] 
        [SerializeField]
        GameObject cameraControls = default;
        
        [FormerlySerializedAs("ProgressTextPanel")] 
        [SerializeField]
        GameObject progressTextPanel = default;

        void OnEnable() {
            PlaybackEventSystem.OnDoneLoadingAnimations += ActivatePlaybackUi;
            PlaybackEventSystem.OnLoadAnimations += ActivateProgressText;
        }

        void OnDisable() {
            PlaybackEventSystem.OnLoadAnimations -= ActivateProgressText;
            PlaybackEventSystem.OnDoneLoadingAnimations -= ActivatePlaybackUi;
        }

        void Start()
        {
            playbackCanvas.SetActive(false);
            animationControls.SetActive(false);
            progressTextPanel.SetActive(false);
            cameraControls.SetActive(false);
        }

        void ActivateProgressText(string unused, string unused2) {
            progressTextPanel.SetActive(true);
        }

        void ActivatePlaybackUi() {
            playbackCanvas.SetActive(true);
            animationControls.SetActive(true);
            cameraControls.SetActive(true);
        }
    }
}
