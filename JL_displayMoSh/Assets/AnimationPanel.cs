
using System.Diagnostics.CodeAnalysis;
using MoshPlayer.Scripts.BML.SMPLModel;
using UnityEngine;

public class AnimationPanel : MonoBehaviour {

    MoshAnimation moshAnimation;
    AnimationControlEvents animationControlEvents;
    AnimationDisplayMainPanel animationDisplayMainPanel;
    
    [SuppressMessage("ReSharper", "ParameterHidesMember")]
    public void Init(MoshAnimation         moshAnimation, AnimationControlEvents animationControlEvents,
                     AnimationDisplayMainPanel animationDisplayMainPanel) {
        this.moshAnimation = moshAnimation;
        this.animationControlEvents = animationControlEvents;
        this.animationDisplayMainPanel = animationDisplayMainPanel;

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
