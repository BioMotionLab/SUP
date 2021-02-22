using System.Collections.Generic;
using Playback;
using UnityEngine;
using UnityEngine.Serialization;

namespace InGameUI {
    public class AnimationDisplayMainPanel : MonoBehaviour {

        [FormerlySerializedAs("AnimationPanelPrefab")] [SerializeField]
        AnimationPanel animationPanelPrefab = default;

        public Dictionary<SUPAnimation, AnimationPanel> OpenPanels;
    
        void OnEnable() {
            OpenPanels = new Dictionary<SUPAnimation, AnimationPanel>();
            AnimationControlEvents.OnAnimationStarted += AddNewAnimationPanel;
        }

        void AddNewAnimationPanel(SUPAnimation supAnimation, AnimationControlEvents animationControlEvents) {
        
            AnimationPanel newAnimationPanel = Instantiate(animationPanelPrefab, transform);
            newAnimationPanel.transform.SetSiblingIndex(0);
            OpenPanels.Add(supAnimation, newAnimationPanel);
            newAnimationPanel.Init(supAnimation, animationControlEvents, this);
        }

        void OnDisable() {
            AnimationControlEvents.OnAnimationStarted -= AddNewAnimationPanel;
        }
    }
}
