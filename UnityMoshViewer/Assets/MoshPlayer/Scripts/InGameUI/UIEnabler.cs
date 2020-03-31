using MoshPlayer.Scripts.Playback;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.InGameUI {
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
            PlaybackEventSystem.OnDoneLoadingAnimations += ActivatePlaybackUi;
            PlaybackEventSystem.OnLoadAnimations += ActivateProgressText;
            PlaybackEventSystem.OnLoadSingleAnimation += ActivateProgressText;
            PlaybackEventSystem.OnLoadNewAnimations += ActivateLoadPanel;
        }

        

        void OnDisable() {
            PlaybackEventSystem.OnLoadAnimations -= ActivateProgressText;
            PlaybackEventSystem.OnDoneLoadingAnimations -= ActivatePlaybackUi;
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

        void ActivateProgressText(string unused, string unused2) {
            ActivatePanel();
        }

        void ActivateProgressText(string unused) {
            ActivatePanel();
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
