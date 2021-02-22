using System;
using System.Diagnostics.CodeAnalysis;
using Playback;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace InGameUI {
    public class AnimationPanel : MonoBehaviour {

        SUPAnimation             supAnimation;
        AnimationControlEvents    animationControlEvents;
        AnimationDisplayMainPanel animationDisplayMainPanel;

        [FormerlySerializedAs("TitleText")] [SerializeField]
        TextMeshProUGUI titleText = default;

        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public void Init(SUPAnimation             supAnimation, AnimationControlEvents animationControlEvents,
                         AnimationDisplayMainPanel animationDisplayMainPanel) {
            this.supAnimation = supAnimation;
            this.animationControlEvents = animationControlEvents;
            this.animationDisplayMainPanel = animationDisplayMainPanel;

            if (supAnimation == null) throw new NullReferenceException("Null moshanimation");
            if (string.IsNullOrEmpty(supAnimation.AnimationName)) throw new NullReferenceException("string empty");
        
            titleText.text = supAnimation.AnimationName;

            FrameSlider slider = GetComponentInChildren<FrameSlider>();
            slider.Init(animationControlEvents); 
            animationControlEvents.OnAnimationEnded += Ended;
        }

        void Ended() {
            animationDisplayMainPanel.OpenPanels.Remove(supAnimation);
            if (gameObject != null) Destroy(gameObject);
            animationControlEvents.OnAnimationEnded -= Ended;
        }
    }
}
