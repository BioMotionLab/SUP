using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BML {
    /// <summary>
    /// View a sequence of MoSh animationSequence read from a file, and record right or left.
    /// </summary>
    public class MoshAnimationPlayer {
	
        readonly List<MoshAnimation[]> animationSequence = new List<MoshAnimation[]>();
        readonly SMPLSettings          settings;


        public bool AllAnimsComplete => currentAnimationIndex >= animationSequence.Count;

        int currentAnimationIndex = 0;
	
        List<MoshCharacter> currentCharacters;

        public MoshAnimationPlayer(List<MoshAnimation[]> animationSequence, SMPLSettings settings) {
            this.animationSequence = animationSequence;
            this.settings = settings;
            currentCharacters = StartAnimation(); //play the first animation!
        }
        

        void StopCurrentAnimation() {
            foreach (MoshCharacter character in currentCharacters) {
                character.StopAnimation();
            }
        }
        
        /// <summary>
        /// Play the animation for both characters at specified position in sequence of files.
        /// </summary>
        /// <param name="animationIndex"></param>
        List<MoshCharacter> StartAnimation() {
            MoshAnimation[] animationGroup = animationSequence[currentAnimationIndex];
            Debug.Log($"Playing animation number {currentAnimationIndex}, {animationGroup.Length} animationSequence in set");
		
            List<MoshCharacter> newCharacters = new List<MoshCharacter>();
            foreach (MoshAnimation moshAnimation in animationGroup) {
                MoshCharacter character = settings.CreateNewCharacter();
                newCharacters.Add(character);
                character.StartAnimation(moshAnimation);
            }

            return newCharacters;
        }

        public void GoToNextAnimation() {
            StopCurrentAnimation();
            currentAnimationIndex++;
            if (AllAnimsComplete) return;
            currentCharacters = StartAnimation();
        }
    }
}