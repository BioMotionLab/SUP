using Playback;
using UnityEngine;

namespace InGameUI {
    public class UIEnabler : MonoBehaviour {

        [SerializeField]
        GameObject playbackCanvas = default;

        [SerializeField] 
        GameObject reviewPanel = default;
        
        [SerializeField]
        GameObject animationControls = default;
        
        [SerializeField]
        GameObject cameraControls = default;
        
        [SerializeField]
        GameObject progressTextPanel = default;

        [SerializeField] 
        GameObject loadPanel = default;
        
        void OnEnable() {
            PlaybackEventSystem.OnBeginPlayBackState += ActivatePlaybackUi;
            PlaybackEventSystem.OnLoadSamples += ActivateProgressText;
            PlaybackEventSystem.OnLoadAnimations += ActivateProgressText;
            PlaybackEventSystem.OnLoadSingleAnimation += ActivateProgressText;
            PlaybackEventSystem.OnLoadNewAnimations += ActivateLoadPanel;
        }

        void ActivateProgressText(string unused) {
            ActivateProgressText();
        }

        void ActivateProgressText(string unused, string unused1) {
            ActivateProgressText();
        }
        
        void ActivateProgressText() {
            ActivatePanel();
        }

        void OnDisable() {
            PlaybackEventSystem.OnLoadAnimations -= ActivateProgressText;
            PlaybackEventSystem.OnLoadSamples -= ActivateProgressText;
            PlaybackEventSystem.OnBeginPlayBackState -= ActivatePlaybackUi;
            PlaybackEventSystem.OnLoadSingleAnimation -= ActivateProgressText;
            PlaybackEventSystem.OnLoadNewAnimations -= ActivateLoadPanel;
        }

        void Start()
        {
            DeactivatePlaybackUi();
        }


        void ActivateLoadPanel() {
            loadPanel.gameObject.SetActive(true);
            DeactivatePlaybackUi();
        }

      

        void ActivatePanel() {
            progressTextPanel.SetActive(true);
        }

        void ActivatePlaybackUi() {
            playbackCanvas.SetActive(true);
            animationControls.SetActive(true);
            cameraControls.SetActive(true);
            reviewPanel.SetActive(true);
        }

        void DeactivatePlaybackUi() {
            playbackCanvas.SetActive(false);
            animationControls.SetActive(false);
            cameraControls.SetActive(false);
            reviewPanel.SetActive(false);
            
            progressTextPanel.SetActive(false);
        }
    }
}
