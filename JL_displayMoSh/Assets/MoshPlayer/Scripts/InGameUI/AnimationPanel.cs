
using System;
using System.Diagnostics.CodeAnalysis;
using MoshPlayer.Scripts.BML.SMPLModel;
using TMPro;
using UnityEngine;

public class AnimationPanel : MonoBehaviour {

    MoshAnimation moshAnimation;
    AnimationControlEvents animationControlEvents;
    AnimationDisplayMainPanel animationDisplayMainPanel;

    [SerializeField]
    TextMeshProUGUI TitleText = default;

    [SuppressMessage("ReSharper", "ParameterHidesMember")]
    public void Init(MoshAnimation         moshAnimation, AnimationControlEvents animationControlEvents,
                     AnimationDisplayMainPanel animationDisplayMainPanel) {
        this.moshAnimation = moshAnimation;
        this.animationControlEvents = animationControlEvents;
        this.animationDisplayMainPanel = animationDisplayMainPanel;

        if (moshAnimation == null) throw new NullReferenceException("Null moshanimation");
        if (string.IsNullOrEmpty(moshAnimation.animationName)) throw new NullReferenceException("string empty");
        
        TitleText.text = moshAnimation.animationName;

        FrameSlider slider = GetComponentInChildren<FrameSlider>();
        slider.Init(animationControlEvents); 
        animationControlEvents.OnAnimationEnded += Ended;
    }

    void Ended() {
        animationDisplayMainPanel.openPanels.Remove(moshAnimation);
        Destroy(gameObject);
        animationControlEvents.OnAnimationEnded -= Ended;
    }
}
