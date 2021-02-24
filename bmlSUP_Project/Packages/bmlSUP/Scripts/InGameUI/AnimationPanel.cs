using System;
using System.Diagnostics.CodeAnalysis;
using Playback;
using TMPro;
using UnityEngine;

namespace InGameUI {
    public class AnimationPanel : MonoBehaviour {

        AMASSAnimation             amassAnimation;
        AnimationControlEvents    animationControlEvents;
        AnimationDisplayMainPanel animationDisplayMainPanel;

        [SerializeField]
        TextMeshProUGUI TitleText = default;

        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public void Init(AMASSAnimation             amassAnimation, AnimationControlEvents animationControlEvents,
                         AnimationDisplayMainPanel animationDisplayMainPanel) {
            this.amassAnimation = amassAnimation;
            this.animationControlEvents = animationControlEvents;
            this.animationDisplayMainPanel = animationDisplayMainPanel;

            if (amassAnimation == null) throw new NullReferenceException("Null moshanimation");
            if (string.IsNullOrEmpty(amassAnimation.AnimationName)) throw new NullReferenceException("string empty");
        
            TitleText.text = amassAnimation.AnimationName;

            FrameSlider slider = GetComponentInChildren<FrameSlider>();
            slider.Init(animationControlEvents); 
            animationControlEvents.OnAnimationEnded += Ended;
        }

        void Ended() {
            animationDisplayMainPanel.OpenPanels.Remove(amassAnimation);
            if (gameObject != null) Destroy(gameObject);
            animationControlEvents.OnAnimationEnded -= Ended;
        }
    }
}
