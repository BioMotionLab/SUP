using System.Collections.Generic;
using JetBrains.Annotations;
using Settings;
using SMPLModel;
using UnityEngine;

namespace Playback {
    
    public class SUPPlayer {
        readonly PlaybackSettings defaultPlaybackSettings;
        readonly DisplaySettings defaultDisplaySettings;
        readonly BodySettings defaultBodySettings;
        List<SMPLCharacter> currentCharacters;
        readonly Transform defaultOrigin;

        /// <summary>
        /// Create a new player object by supplying default settings. These settings can be overriden when playing animations.
        /// </summary>
        /// <param name="defaultPlaybackSettings"></param>
        /// <param name="defaultDisplaySettings"></param>
        /// <param name="defaultBodySettings"></param>
        /// <param name="defaultOrigin">Optionally provide an origin transform to reposition the characters in the scene.</param>
        public SUPPlayer(PlaybackSettings defaultPlaybackSettings, DisplaySettings defaultDisplaySettings, BodySettings defaultBodySettings, Transform defaultOrigin = null) {
            this.defaultPlaybackSettings = defaultPlaybackSettings;
            this.defaultDisplaySettings = defaultDisplaySettings;
            this.defaultBodySettings = defaultBodySettings;
            this.defaultOrigin = defaultOrigin;
        }
        
        
        /// <summary>
        /// Play a set of multiple animations simultaneously
        /// </summary>
        /// <param name="animationGroup">The set to play</param>
        /// <param name="overrideDisplaySettings">Optionally override settings for this set only</param>
        /// <param name="overridePlaybackSettings">Optionally override settings for this set only</param>
        /// <param name="overrideBodySettings">Optionally override settings for this set only</param>
        /// <param name="overrideOrigin">Optionally provide an origin transform to reposition the characters in the scene.</param>
        [PublicAPI]
        public void Play(List<AMASSAnimation> animationGroup, 
            DisplaySettings overrideDisplaySettings = null, 
            PlaybackSettings overridePlaybackSettings = null, 
            BodySettings overrideBodySettings = null, 
            Transform overrideOrigin = null) {

            DisplaySettings playWithDisplaySettings = overrideDisplaySettings != null ? overrideDisplaySettings : defaultDisplaySettings;
            PlaybackSettings playWithPlaybackSettings = overridePlaybackSettings != null ? overridePlaybackSettings : defaultPlaybackSettings;
            BodySettings playWithBodySettings = overrideBodySettings != null ? overrideBodySettings : defaultBodySettings;
            Transform playWithOrigin = overrideOrigin != null ? overrideOrigin : defaultOrigin;
            
            List<SMPLCharacter> newCharacters = new List<SMPLCharacter>();

            for (int animationIndex = 0; animationIndex < animationGroup.Count; animationIndex++) {
				
                AMASSAnimation amassAnimation = animationGroup[animationIndex];
                amassAnimation.Reset();

                SMPLCharacter smplCharacter =
                    amassAnimation.Data.Model.CreateCharacter(amassAnimation, animationIndex);

                newCharacters.Add(smplCharacter);
                smplCharacter.StartAnimation(amassAnimation, playWithPlaybackSettings, playWithDisplaySettings,
                    playWithBodySettings);

                smplCharacter.SetOrigin(playWithOrigin);

            }
			
            currentCharacters = newCharacters;
        }
        
        
        /// <summary>
        /// Plays a single animation
        /// </summary>
        /// <param name="animation">The animation to play</param>
        /// <param name="overrideDisplaySettings">Optionally override settings for this animation only</param>
        /// <param name="overridePlaybackSettings">Optionally override settings for this animation only</param>
        /// <param name="overrideBodySettings">Optionally override settings for this animation only</param>
        /// <param name="overrideOrigin">Optionally provide an origin transform to reposition the character in the scene.</param>
        [PublicAPI]
        public void Play(AMASSAnimation animation, 
                        DisplaySettings overrideDisplaySettings = null, 
                        PlaybackSettings overridePlaybackSettings = null, 
                        BodySettings overrideBodySettings = null, 
                        Transform overrideOrigin = null) {
            List<AMASSAnimation> asList = new List<AMASSAnimation> {animation};
            
            Play(asList, 
                overrideDisplaySettings, 
                overridePlaybackSettings, 
                overrideBodySettings, 
                overrideOrigin);
        }
        
        /// <summary>
        /// Immediately stop all currently playing animations. Existing characters will be removed from the scene
        /// </summary>
        [PublicAPI]
        public void StopCurrentAnimations() {
            if (currentCharacters == null) return;
            foreach (SMPLCharacter character in currentCharacters) {
                if (character == null) continue;
                character.InterruptAnimation();
            }
        }
        
    }
}