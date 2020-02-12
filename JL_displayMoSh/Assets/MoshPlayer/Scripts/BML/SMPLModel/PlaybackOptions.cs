using System;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    [Serializable]
    public class PlaybackOptions {
        
        [Range(0,5)]
        public float DisplaySpeed = 1f;
        public bool PlayBackwards = false;
        public int FallbackFPS = 120;
        
        //TODO Loop
        public bool Loop;
    }
}