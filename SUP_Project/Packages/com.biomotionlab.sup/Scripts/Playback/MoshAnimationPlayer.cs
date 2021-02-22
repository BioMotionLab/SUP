using System.Collections.Generic;
using Display;
using MoshPlayer;
using SMPLModel;

namespace Playback {
    
    public class MoshAnimationPlayer {
        readonly PlaybackSettings playbackSettings;
        readonly DisplaySettings displaySettings;
        readonly BodyOptions bodyOptions;
        List<MoshCharacter> currentCharacters;

        public MoshAnimationPlayer(PlaybackSettings playbackSettings, DisplaySettings displaySettings, BodyOptions bodyOptions) {
            this.playbackSettings = playbackSettings;
            this.displaySettings = displaySettings;
            this.bodyOptions = bodyOptions;
        }

        public void PlaySet(List<MoshAnimation> animationGroup) {
            
            List<MoshCharacter> newCharacters = new List<MoshCharacter>();

            for (int animationIndex = 0; animationIndex < animationGroup.Count; animationIndex++) {
				
                MoshAnimation moshAnimation = animationGroup[animationIndex];
                moshAnimation.Reset();
				
                MoshCharacter moshCharacter =
                    moshAnimation.Data.Model.CreateCharacter(moshAnimation, animationIndex);

                newCharacters.Add(moshCharacter);
                moshCharacter.StartAnimation(moshAnimation, playbackSettings, displaySettings,
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
            foreach (MoshCharacter character in currentCharacters) {
                if (character == null) continue;
                character.InterruptAnimation();
            }
        }
    }
}