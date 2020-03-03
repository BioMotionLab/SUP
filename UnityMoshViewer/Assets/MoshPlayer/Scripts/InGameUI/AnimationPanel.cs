using System;
using System.Diagnostics.CodeAnalysis;
using MoshPlayer.Scripts.Playback;
using TMPro;
using UnityEngine;

namespace MoshPlayer.Scripts.InGameUI {
    public class AnimationPanel : MonoBehaviour {

        MoshAnimation             moshAnimation;
        AnimationControlEvents    animationControlEvents;
        AnimationDisplayMainPanel animationDisplayMainPanel;

        [SerializeField]
        TextMeshProUGUI TitleText = default;

        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public void Init(MoshAnimation             moshAnimation, AnimationControlEvents animationControlEvents,
                         AnimationDisplayMainPanel animationDisplayMainPanel) {
            this.moshAnimation = moshAnimation;
            this.animationControlEvents = animationControlEvents;
            this.animationDisplayMainPanel = animationDisplayMainPanel;

            if (moshAnimation == null) throw new NullReferenceException("Null moshanimation");
            if (string.IsNullOrEmpty(moshAnimation.AnimationName)) throw new NullReferenceException("string empty");
        
            TitleText.text = moshAnimation.AnimationName;

            FrameSlider slider = GetComponentInChildren<FrameSlider>();
            slider.Init(animationControlEvents); 
            animationControlEvents.OnAnimationEnded += Ended;
        }

        void Ended() {
            animationDisplayMainPanel.OpenPanels.Remove(moshAnimation);
            Destroy(gameObject);
            animationControlEvents.OnAnimationEnded -= Ended;
        }
    }
}
