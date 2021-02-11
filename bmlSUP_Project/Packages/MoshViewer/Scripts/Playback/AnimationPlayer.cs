using System.Collections.Generic;
using Display;
using MoshPlayer.Scripts.SMPLModel;
using Playback;
using Settings;

namespace MoshPlayer.Scripts.Playback {
    
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

        public void PlaySet(List<AMASSAnimation> animationGroup) {
            
            List<MoshCharacter> newCharacters = new List<MoshCharacter>();

            for (int animationIndex = 0; animationIndex < animationGroup.Count; animationIndex++) {
				
                AMASSAnimation amassAnimation = animationGroup[animationIndex];
                amassAnimation.Reset();
				
                MoshCharacter moshCharacter =
                    amassAnimation.Data.Model.CreateCharacter(amassAnimation, animationIndex);

                newCharacters.Add(moshCharacter);
                moshCharacter.StartAnimation(amassAnimation, playbackSettings, displaySettings,
                    bodyOptions);
            }
			
            currentCharacters = newCharacters;
        }

        public void Play(AMASSAnimation animation) {
            List<AMASSAnimation> asList = new List<AMASSAnimation> {animation};
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