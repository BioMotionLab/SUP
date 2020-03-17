using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.AnimationReviewer {
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
        }

    }
}
