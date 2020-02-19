
using MoshPlayer.Scripts.Playback;
using UnityEngine;

public class UIEnabler : MonoBehaviour {

    [SerializeField]
    GameObject PlaybackCanvas = default;

    [SerializeField]
    GameObject AnimationControls = default;

    
    [SerializeField]
    GameObject CameraControls = default;

    
    [SerializeField]
    GameObject ProgressTextPanel = default;

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
        PlaybackCanvas.SetActive(false);
        AnimationControls.SetActive(false);
        ProgressTextPanel.SetActive(false);
        CameraControls.SetActive(false);
    }

    void ActivateProgressText(string unused, string unused2) {
        ProgressTextPanel.SetActive(true);
    }

    void ActivatePlaybackUi() {
        PlaybackCanvas.SetActive(true);
        AnimationControls.SetActive(true);
        CameraControls.SetActive(true);
    }
}
