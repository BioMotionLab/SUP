using System.Collections.Generic;
using Display;
using Settings;
using SMPLModel;

namespace Playback {
    
    public class MoshAnimationPlayer {
        readonly PlaybackSettings playbackSettings;
        readonly DisplaySettings displaySettings;
        readonly BodyOptions bodyOptions;
        List<SMPLCharacter> currentCharacters;

        public MoshAnimationPlayer(PlaybackSettings playbackSettings, DisplaySettings displaySettings, BodyOptions bodyOptions) {
            this.playbackSettings = playbackSettings;
            this.displaySettings = displaySettings;
            this.bodyOptions = bodyOptions;
        }

        public void PlaySet(List<MoshAnimation> animationGroup) {
            
            List<SMPLCharacter> newCharacters = new List<SMPLCharacter>();

            for (int animationIndex = 0; animationIndex < animationGroup.Count; animationIndex++) {
				
                MoshAnimation moshAnimation = animationGroup[animationIndex];
                moshAnimation.Reset();
				
                SMPLCharacter smplCharacter =
                    moshAnimation.Data.Model.CreateCharacter(moshAnimation, animationIndex);

                newCharacters.Add(smplCharacter);
                smplCharacter.StartAnimation(moshAnimation, playbackSettings, displaySettings,
                    bodyOptions);
            }
			
            currentCharacters = newCharacters;
        }

        public void Play(MoshAnimation animation) {
            List<MoshAnimation> asList = new List<MoshAnimation> {animation};
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