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
	
        List<MoshCharacterComponent> currentCharacters;
        DisplayPointLights displayPointLights;
        DisplayMesh displayMesh;
        DisplayBones displayBones;

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
            foreach (MoshCharacterComponent character in currentCharacters) {
                if (character == null) continue;
                character.InterruptAnimation();
            }
        }
        
        /// <summary>
        /// Play the animation for both characters at specified position in sequence of files.
        /// </summary>
        List<MoshCharacterComponent> StartAnimation() {
            List<MoshAnimation> animationGroup = animationSequence[currentAnimationIndex];

            string backwardsText = settings.PlayBackwards ? "backwards" : "";
            Debug.Log($"Playing animation {currentAnimationIndex+1} of {animationSequence.Count}. " +
                      $"Contains animations for {animationGroup.Count} characters. Playing at {settings.DisplaySpeed}X speed {backwardsText}.");
		
            List<MoshCharacterComponent> newCharacters = new List<MoshCharacterComponent>();
            for (int animationIndex = 0; animationIndex < animationGroup.Count; animationIndex++) {
                MoshAnimation moshAnimation = animationGroup[animationIndex];
                MoshCharacterComponent characterComponent = settings.CreateNewCharacter();
                characterComponent.name = $"Character {animationIndex}";
                if (displayPointLights == DisplayPointLights.On) {
                    DisplaySMPLPointLights pointLightDisplay = characterComponent.gameObject.AddComponent<DisplaySMPLPointLights>();
                    pointLightDisplay.Init(characterComponent, settings);
                }
                if (displayBones == DisplayBones.On)
                {
                    DisplaySMPLBones boneDisplay = characterComponent.gameObject.AddComponent<DisplaySMPLBones>();
                    boneDisplay.Init(characterComponent, settings);
                }

                if (displayMesh == DisplayMesh.Off) {
                    characterComponent.SkinnedMeshRender.enabled = false;
                }

                if (displayMesh == DisplayMesh.SemiTransparent) {
                    characterComponent.SkinnedMeshRender.material = settings.DisplaySettings.SemiTransparentMaterial;
                }
                newCharacters.Add(characterComponent);
                characterComponent.StartAnimation(moshAnimation, settings);
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