using System.Collections.Generic;
using Playback;
using UnityEngine;
using UnityEngine.Serialization;

namespace InGameUI {
    public class AnimationDisplayMainPanel : MonoBehaviour {

        [FormerlySerializedAs("AnimationPanelPrefab")] [SerializeField]
        AnimationPanel animationPanelPrefab = default;

        public Dictionary<MoshAnimation, AnimationPanel> OpenPanels;
    
        void OnEnable() {
            OpenPanels = new Dictionary<MoshAnimation, AnimationPanel>();
            AnimationControlEvents.OnAnimationStarted += AddNewAnimationPanel;
        }

        void AddNewAnimationPanel(MoshAnimation moshAnimation, AnimationControlEvents animationControlEvents) {
        
            AnimationPanel newAnimationPanel = Instantiate(animationPanelPrefab, transform);
            newAnimationPanel.transform.SetSiblingIndex(0);
            OpenPanels.Add(moshAnimation, newAnimationPanel);
            newAnimationPanel.Init(moshAnimation, animationControlEvents, this);
        }

        void OnDisable() {
            AnimationControlEvents.OnAnimationStarted -= AddNewAnimationPanel;
        }
    }
}
