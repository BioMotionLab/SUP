using System;
using System.Diagnostics.CodeAnalysis;
using Playback;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace InGameUI {
    public class AnimationPanel : MonoBehaviour {

        MoshAnimation             moshAnimation;
        AnimationControlEvents    animationControlEvents;
        AnimationDisplayMainPanel animationDisplayMainPanel;

        [FormerlySerializedAs("TitleText")] [SerializeField]
        TextMeshProUGUI titleText = default;

        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public void Init(MoshAnimation             moshAnimation, AnimationControlEvents animationControlEvents,
                         AnimationDisplayMainPanel animationDisplayMainPanel) {
            this.moshAnimation = moshAnimation;
            this.animationControlEvents = animationControlEvents;
            this.animationDisplayMainPanel = animationDisplayMainPanel;

            if (moshAnimation == null) throw new NullReferenceException("Null moshanimation");
            if (string.IsNullOrEmpty(moshAnimation.AnimationName)) throw new NullReferenceException("string empty");
        
            titleText.text = moshAnimation.AnimationName;

            FrameSlider slider = GetComponentInChildren<FrameSlider>();
            slider.Init(animationControlEvents); 
            animationControlEvents.OnAnimationEnded += Ended;
        }

        void Ended() {
            animationDisplayMainPanel.OpenPanels.Remove(moshAnimation);
            if (gameObject != null) Destroy(gameObject);
            animationControlEvents.OnAnimationEnded -= Ended;
        }
    }
}
