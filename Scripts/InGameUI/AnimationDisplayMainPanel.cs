using System.Collections.Generic;
using MoshPlayer.Scripts.Playback;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.InGameUI {
    public class AnimationDisplayMainPanel : MonoBehaviour {

        [SerializeField]
        AnimationPanel AnimationPanelPrefab = default;

        [FormerlySerializedAs("openPanels")]
        public Dictionary<MoshAnimation, AnimationPanel> OpenPanels;
    
        void OnEnable() {
            OpenPanels = new Dictionary<MoshAnimation, AnimationPanel>();
            AnimationControlEvents.OnAnimationStarted += AddNewAnimationPanel;
        }

        void AddNewAnimationPanel(MoshAnimation moshAnimation, AnimationControlEvents animationControlEvents) {
        
            AnimationPanel newAnimationPanel = Instantiate(AnimationPanelPrefab, transform);
            newAnimationPanel.transform.SetSiblingIndex(0);
            OpenPanels.Add(moshAnimation, newAnimationPanel);
            newAnimationPanel.Init(moshAnimation, animationControlEvents, this);
        }

        void OnDisable() {
            AnimationControlEvents.OnAnimationStarted -= AddNewAnimationPanel;
        }
    }
}
