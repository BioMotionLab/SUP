using System;
using System.Collections;
using System.Collections.Generic;
using MoshPlayer.Scripts.BML.SMPLModel;
using UnityEngine;

public class AnimationDisplayMainPanel : MonoBehaviour {

    [SerializeField]
    AnimationPanel AnimationPanelPrefab = default;

    public Dictionary<MoshAnimation, AnimationPanel> openPanels;
    
    void OnEnable() {
        openPanels = new Dictionary<MoshAnimation, AnimationPanel>();
        AnimationControlEvents.OnAnimationStarted += AddNewAnimationPanel;
    }

    void AddNewAnimationPanel(MoshAnimation moshAnimation, AnimationControlEvents animationControlEvents) {
        
        AnimationPanel newAnimationPanel = Instantiate(AnimationPanelPrefab, this.transform);
        newAnimationPanel.transform.SetSiblingIndex(0);
        openPanels.Add(moshAnimation, newAnimationPanel);
        newAnimationPanel.Init(moshAnimation, animationControlEvents, this);
    }

    void OnDisable() {
        AnimationControlEvents.OnAnimationStarted -= AddNewAnimationPanel;
    }
}
