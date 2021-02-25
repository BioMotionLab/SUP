using System.Collections.Generic;
using Display;
using Settings;
using SMPLModel;

namespace Playback {
    
    public class SUPAnimationPlayer {
        readonly PlaybackSettings playbackSettings;
        readonly DisplaySettings displaySettings;
        readonly BodyOptions bodyOptions;
        List<SMPLCharacter> currentCharacters;

        public SUPAnimationPlayer(PlaybackSettings playbackSettings, DisplaySettings displaySettings, BodyOptions bodyOptions) {
            this.playbackSettings = playbackSettings;
            this.displaySettings = displaySettings;
            this.bodyOptions = bodyOptions;
        }

        public void PlaySet(List<SUPAnimation> animationGroup) {
            
            List<SMPLCharacter> newCharacters = new List<SMPLCharacter>();

            for (int animationIndex = 0; animationIndex < animationGroup.Count; animationIndex++) {
				
                SUPAnimation supAnimation = animationGroup[animationIndex];
                supAnimation.Reset();
				
                SMPLCharacter smplCharacter =
                    supAnimation.Data.Model.CreateCharacter(supAnimation, animationIndex);

                newCharacters.Add(smplCharacter);
                smplCharacter.StartAnimation(supAnimation, playbackSettings, displaySettings,
                    bodyOptions);
            }
			
            currentCharacters = newCharacters;
        }

        public void Play(SUPAnimation animation) {
            List<SUPAnimation> asList = new List<SUPAnimation> {animation};
            PlaySet(asList);
        }
        
        public void StopCurrentAnimations() {
            if (currentCharacters == null) return;
            foreach (SMPLCharacter character in currentCharacters) {
                if (character == null) continue;
                character.InterruptAnimation();
            }
        }
    }
}