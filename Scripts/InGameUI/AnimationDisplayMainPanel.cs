using System.Collections.Generic;
using Playback;
using UnityEngine;
using UnityEngine.Serialization;

namespace InGameUI {
    public class AnimationDisplayMainPanel : MonoBehaviour {

        [FormerlySerializedAs("AnimationPanelPrefab")] [SerializeField]
        AnimationPanel animationPanelPrefab = default;

        [FormerlySerializedAs("openPanels")]
        public Dictionary<AMASSAnimation, AnimationPanel> OpenPanels;
    
        void OnEnable() {
            OpenPanels = new Dictionary<AMASSAnimation, AnimationPanel>();
            AnimationControlEvents.OnAnimationStarted += AddNewAnimationPanel;
        }

        void AddNewAnimationPanel(AMASSAnimation amassAnimation, AnimationControlEvents animationControlEvents) {
        
            AnimationPanel newAnimationPanel = Instantiate(animationPanelPrefab, transform);
            newAnimationPanel.transform.SetSiblingIndex(0);
            OpenPanels.Add(amassAnimation, newAnimationPanel);
            newAnimationPanel.Init(amassAnimation, animationControlEvents, this);
        }

        void OnDisable() {
            AnimationControlEvents.OnAnimationStarted -= AddNewAnimationPanel;
        }
    }
}
