using System.Collections.Generic;
using MoshPlayer.Scripts.BML.Display;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    /// <summary>
    /// View a sequence of MoSh animationSequence read from a file, and record right or left.
    /// </summary>
    public class MoshAnimationPlayer {
	
        readonly List<List<MoshAnimation>> animationSequence;
        readonly SMPLSettings          settings;
        
        public bool AllAnimsComplete => currentAnimationIndex >= animationSequence.Count;

        int currentAnimationIndex = 0;
	
        List<MoshCharacter> currentCharacters;
        readonly DisplayPointLights displayPointLights;
        readonly DisplayMesh displayMesh;
        readonly DisplayBones displayBones;

        public MoshAnimationPlayer(List<List<MoshAnimation>> animationSequence, SMPLSettings settings,
                                   DisplayPointLights displayPointLights, DisplayBones displayBones, DisplayMesh displayMesh) {
            this.displayPointLights = displayPointLights;
            this.displayBones = displayBones;
            this.displayMesh = displayMesh;
            this.animationSequence = animationSequence;
            this.settings = settings;
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

            string backwardsText = settings.PlayBackwards ? "backwards" : "";
            Debug.Log($"Playing animation {currentAnimationIndex+1} of {animationSequence.Count}. " +
                      $"Contains animations for {animationGroup.Count} characters. Playing at {settings.DisplaySpeed}X speed {backwardsText}.");
		
            List<MoshCharacter> newCharacters = new List<MoshCharacter>();
            for (int animationIndex = 0; animationIndex < animationGroup.Count; animationIndex++) {
                MoshAnimation moshAnimation = animationGroup[animationIndex];
                string characterName = $"Character {animationIndex}";
                MoshCharacter moshCharacter = settings.CreateNewCharacter(characterName);
                if (displayPointLights == DisplayPointLights.On) {
                    DisplaySMPLPointLights pointLightDisplay = moshCharacter.gameObject.AddComponent<DisplaySMPLPointLights>();
                    pointLightDisplay.Init(moshCharacter, settings);
                }
                if (displayBones == DisplayBones.On)
                {
                    DisplaySMPLBones boneDisplay = moshCharacter.gameObject.AddComponent<DisplaySMPLBones>();
                    boneDisplay.Init(moshCharacter, settings);
                }

                if (displayMesh == DisplayMesh.Off) {
                    moshCharacter.SkinnedMeshRender.enabled = false;
                }

                if (displayMesh == DisplayMesh.SemiTransparent) {
                    moshCharacter.SkinnedMeshRender.material = settings.DisplaySettings.SemiTransparentMaterial;
                }
                newCharacters.Add(moshCharacter);
                moshCharacter.StartAnimation(moshAnimation, settings);
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