﻿using System;
using MoshPlayer.Scripts.Playback;
using UnityEngine;

namespace MoshPlayer.Scripts.InGameUI {
    public class RenderSelector : MonoBehaviour {

        [SerializeField] GameObject manualPoseManipulationPanel;

        void Awake() {
            if (!Application.isEditor) manualPoseManipulationPanel.SetActive(false);
        }

        public void ChangeLiveBodyShape(bool changeShape) {
            PlaybackEventSystem.ChangeLiveBodyShapeRendering(changeShape);
        }
    

        public void ChangeLivePoses(bool livePoses) {
            PlaybackEventSystem.ChangeLivePoseRendering(livePoses);
        }
    

        public void ChangeLivePoseBlendshapes(bool livePoseBlendshapes) {
            PlaybackEventSystem.ChangeLivePoseBlendshapeRendering(livePoseBlendshapes);
        }
    

        public void ChangeManualPosing(bool manualPosing) {
            PlaybackEventSystem.ChangeManualPosing(manualPosing);
        }

        public void ChangeSnapToGround(bool snapToGround) {
            PlaybackEventSystem.ChangeSnapToGround(snapToGround);
        }

        public void ChangeUpdateYTranslation(bool changeUpdateYTranslation) {
            PlaybackEventSystem.ChangeUpdateYTranslation(changeUpdateYTranslation);
        }

  
        public void ChangeUpdateXzTranslation(bool changeUpdateXzTranslation) {
            PlaybackEventSystem.ChangeUpdateXzTranslation(changeUpdateXzTranslation);
        }
    
    
    }
}
