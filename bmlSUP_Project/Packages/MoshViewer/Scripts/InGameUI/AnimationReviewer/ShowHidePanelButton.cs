using JetBrains.Annotations;
    using UnityEngine;
    using UnityEngine.Serialization;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
#endif

namespace InGameUI.AnimationReviewer {
    public class ShowHidePanelButton : MonoBehaviour {

        [FormerlySerializedAs("ReviewPanel")]
        [FormerlySerializedAs("LinkedPanel")]
        [SerializeField]
        GameObject linkedPanel = default;
        
        [SerializeField]
        bool startEnabled = default;

        public void OnEnable() {
            SetState(startEnabled);
        }

        [PublicAPI]
        public void ToggleShow() {
            SetState(!linkedPanel.activeSelf);
        }

        void SetState(bool state) {
            linkedPanel.SetActive(state);
            if (state) KeyboardControlEvents.DisableKeyboardControls();
            else KeyboardControlEvents.EnableKeyboardControls();
        }

    }
}
