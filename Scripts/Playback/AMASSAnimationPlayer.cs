using System.Collections.Generic;
using Display;
using Settings;
using SMPLModel;

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

        public void PlayAnimationSet(List<AMASSAnimation> animationGroup) {
            
            List<SMPLCharacter> newCharacters = new List<SMPLCharacter>();

            for (int animationIndex = 0; animationIndex < animationGroup.Count; animationIndex++) {
				
                AMASSAnimation amassAnimation = animationGroup[animationIndex];
                amassAnimation.Reset();
				
                SMPLCharacter smplCharacter =
                    amassAnimation.Data.Model.CreateCharacter(amassAnimation, animationIndex);

                newCharacters.Add(smplCharacter);
                smplCharacter.StartAnimation(amassAnimation, playbackSettings, displaySettings,
                    bodyOptions);
            }
			
            currentCharacters = newCharacters;
        }

        public void Play(AMASSAnimation animation) {
            List<AMASSAnimation> asList = new List<AMASSAnimation> {animation};
            PlayAnimationSet(asList);
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