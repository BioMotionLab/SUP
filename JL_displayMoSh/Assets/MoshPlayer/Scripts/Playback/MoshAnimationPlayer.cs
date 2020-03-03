using System;
using System.Collections.Generic;
using MoshPlayer.Scripts.SMPLModel;
using UnityEngine;

namespace MoshPlayer.Scripts.Playback {
    /// <summary>
    /// View a sequence of MoSh animationSequence read from a file, and record right or left.
    /// </summary>
    public class MoshAnimationPlayer {
	
        readonly List<List<MoshAnimation>> animationSequence;
        readonly PlaybackOptions playbackOptions;
        readonly CharacterDisplayOptions displayOptions;
        readonly CharacterRenderOptions renderOptions;

        public bool AllAnimsComplete => currentAnimationIndex >= animationSequence.Count;

        int currentAnimationIndex = 0;
	
        List<MoshCharacter> currentCharacters;
        

        public MoshAnimationPlayer(List<List<MoshAnimation>> animationSequence, PlaybackOptions playbackOptions, CharacterDisplayOptions displayOptions, CharacterRenderOptions renderOptions) {
            this.animationSequence = animationSequence;
            this.playbackOptions = playbackOptions;
            this.displayOptions = displayOptions;
            this.renderOptions = renderOptions;
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

            string updateMessage = $"Playing animation set {currentAnimationIndex+1} of {animationSequence.Count}. " +
                             $"({animationGroup.Count} chars)";
            Debug.Log(updateMessage);
            PlaybackEventSystem.UpdatePlayerProgress(updateMessage);

            string animations = "";
            
            List<MoshCharacter> newCharacters = new List<MoshCharacter>();
            for (int animationIndex = 0; animationIndex < animationGroup.Count; animationIndex++) {
                MoshAnimation moshAnimation = animationGroup[animationIndex];
                animations += moshAnimation.AnimationName + " ";
                
                string characterName = $"{moshAnimation.Gender} Character {animationIndex}";
                MoshCharacter moshCharacter = moshAnimation.Model.CreateNewCharacter(characterName, moshAnimation.Gender);
                
                newCharacters.Add(moshCharacter);
                moshCharacter.StartAnimation(moshAnimation, playbackOptions, displayOptions, renderOptions);
            }
            PlaybackEventSystem.PlayingNewAnimationSet(animations.Trim());
            
            return newCharacters;
        }

        public void GoToNextAnimation() {
            StopCurrentAnimations();
            currentAnimationIndex++;
            if (AllAnimsComplete) {
                string updateMessage = "All Animations Complete";
                Debug.Log(updateMessage);
                PlaybackEventSystem.UpdatePlayerProgress(updateMessage);
                return;
            }
            currentCharacters = StartAnimation();
        }
    }
}