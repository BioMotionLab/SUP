using System;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    [Serializable]
    public class PlaybackOptions {
        
        public bool PlayBackwards = false;
        public int FallbackFPS = 120;
        
        //TODO Loop
        public bool Loop;
    }
}