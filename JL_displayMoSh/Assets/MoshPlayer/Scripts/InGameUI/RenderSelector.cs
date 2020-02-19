﻿using JetBrains.Annotations;
using MoshPlayer.Scripts.Playback;
using UnityEngine;

namespace MoshPlayer.Scripts.InGameUI {
    public class RenderSelector : MonoBehaviour
    {


        [PublicAPI]
        public void ChangeLiveBodyShape(bool changeShape) {
            PlaybackEventSystem.ChangeLiveBodyShapeRendering(changeShape);
        }
    
        [PublicAPI]
        public void ChangeLivePoses(bool livePoses) {
            PlaybackEventSystem.ChangeLivePoseRendering(livePoses);
        }
    
        [PublicAPI]
        public void ChangeLivePoseBlendshapes(bool livePoseBlendshapes) {
            PlaybackEventSystem.ChangeLivePoseBlendshapeRendering(livePoseBlendshapes);
        }
    
        [PublicAPI]
        public void ChangeManualPosing(bool manualPosing) {
            PlaybackEventSystem.ChangeManualPosing(manualPosing);
        }
    
    
    }
}
