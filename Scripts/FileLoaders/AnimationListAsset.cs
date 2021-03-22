using System;
using System.Collections.Generic;
using Playback;
using SMPLModel;
using UnityEngine;

namespace FileLoaders {
    
    [CreateAssetMenu]
    public class AnimationListAsset : ScriptableObject {
        
        public PlaybackSettings playbackSettings = default;
        public Models models = default;
        
        public List<AnimationAssetGroup> animationAssetGroups = default;


        public PlaybackSettings PlaybackSettings => playbackSettings;

        public Models Models => models;

        public List<AnimationAssetGroup> AnimationAssetGroups => animationAssetGroups;
    }

    [Serializable]
    public class AnimationAssetGroup {
        [SerializeField]
        public List<TextAsset> assets = default;
    }
}
