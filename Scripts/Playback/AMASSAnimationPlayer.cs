using System.Collections.Generic;
using Display;
using Settings;
using SMPLModel;
using UnityEngine;

namespace Playback {
    
    public class AMASSAnimationPlayer {
        readonly PlaybackSettings playbackSettings;
        DisplaySettings displaySettings;
        readonly BodyOptions bodyOptions;
        List<SMPLCharacter> currentCharacters;

        public AMASSAnimationPlayer(PlaybackSettings playbackSettings, DisplaySettings displaySettings, BodyOptions bodyOptions) {
            this.playbackSettings = playbackSettings;
            this.displaySettings = displaySettings;
            this.bodyOptions = bodyOptions;
        }

        public void Play(List<AMASSAnimation> animationGroup, Transform origin = null) {
            
            List<SMPLCharacter> newCharacters = new List<SMPLCharacter>();

            for (int animationIndex = 0; animationIndex < animationGroup.Count; animationIndex++) {
				
                AMASSAnimation amassAnimation = animationGroup[animationIndex];
                amassAnimation.Reset();

                Transform testOrigin = new GameObject().transform;
                testOrigin.position = Vector3.one*3;
                testOrigin.eulerAngles = new Vector3(0, 180, 0);
                
                SMPLCharacter smplCharacter =
                    amassAnimation.Data.Model.CreateCharacter(amassAnimation, animationIndex);

                newCharacters.Add(smplCharacter);
                smplCharacter.StartAnimation(amassAnimation, playbackSettings, displaySettings,
                    bodyOptions);

                smplCharacter.SetOrigin(testOrigin);

            }
			
            currentCharacters = newCharacters;
        }

        public void Play(AMASSAnimation animation, Transform origin = null) {
            List<AMASSAnimation> asList = new List<AMASSAnimation> {animation};
            Play(asList, origin);
        }
        
        public void StopCurrentAnimations() {
            if (currentCharacters == null) return;
            foreach (SMPLCharacter character in currentCharacters) {
                if (character == null) continue;
                character.InterruptAnimation();
            }
        }

        public void SetDisplaySettings(DisplaySettings newDisplaySettings) {
            displaySettings = newDisplaySettings;
        }
    }
}