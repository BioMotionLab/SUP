using System.Collections.Generic;
using MoshPlayer.Scripts.BML.Display;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    /// <summary>
    /// View a sequence of MoSh animationSequence read from a file, and record right or left.
    /// </summary>
    public class MoshAnimationPlayer {
	
        readonly List<List<MoshAnimation>> animationSequence;
        readonly SettingsMain          settingsMain;
        
        public bool AllAnimsComplete => currentAnimationIndex >= animationSequence.Count;

        int currentAnimationIndex = 0;
	
        List<MoshCharacter> currentCharacters;
        

        public MoshAnimationPlayer(List<List<MoshAnimation>> animationSequence, SettingsMain settingsMain) {
            this.animationSequence = animationSequence;
            this.settingsMain = settingsMain;
        }


        public void StartPlayingAnimations() {
            currentCharacters = StartAnimation(); //play the first animation!
        }

        void StopCurrentAnimations() {
            foreach (MoshCharacter character in currentCharacters) {
                if (character == null) continue;
                character.InterruptAnimation();
            }
        }
        
        /// <summary>
        /// Play the animation for both characters at specified position in sequence of files.
        /// </summary>
        List<MoshCharacter> StartAnimation() {
            List<MoshAnimation> animationGroup = animationSequence[currentAnimationIndex];

            string backwardsText = settingsMain.DisplaySettings.PlayBackwards ? "backwards" : "";
            Debug.Log($"Playing animation {currentAnimationIndex+1} of {animationSequence.Count}. " +
                      $"Contains animations for {animationGroup.Count} characters. Playing at {settingsMain.DisplaySettings.DisplaySpeed}X speed {backwardsText}.");
		
            List<MoshCharacter> newCharacters = new List<MoshCharacter>();
            for (int animationIndex = 0; animationIndex < animationGroup.Count; animationIndex++) {
                MoshAnimation moshAnimation = animationGroup[animationIndex];
                string characterName = $"{moshAnimation.Gender} Character {animationIndex}";
                MoshCharacter moshCharacter = moshAnimation.Model.CreateNewCharacter(characterName, moshAnimation.Gender);
                
                

                newCharacters.Add(moshCharacter);
                moshCharacter.StartAnimation(moshAnimation, settingsMain);
            }

            return newCharacters;
        }

        public void GoToNextAnimation() {
            StopCurrentAnimations();
            currentAnimationIndex++;
            if (AllAnimsComplete) {
                Debug.Log("All Animations Complete");
                return;
            }
            currentCharacters = StartAnimation();
        }
    }
}