using System;
using System.Collections.Generic;
using Playback;
using Settings;
using SMPLModel;
using UnityEngine;

namespace FileLoaders {
    
    [CreateAssetMenu(fileName = "New Animation List Asset", menuName = Menu.AssetMenu + "New AnimationList Asset")]
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
