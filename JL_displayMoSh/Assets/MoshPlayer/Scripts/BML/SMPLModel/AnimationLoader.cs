using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MoshPlayer.Scripts.BML.FileLoaders;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    public class AnimationLoader : MonoBehaviour {
		
        string       animationsToPlayFile;
        SMPLSettings settings;
        string       animFolder;

        public bool                  DoneLoading;
        public List<MoshAnimation[]> animationSequence;
        string[]                     animLines;
        Action<List<MoshAnimation[]>>                       doneAction;


        public void Init(string animationsToPlayFile, SMPLSettings settings, string animFolder, Action<List<MoshAnimation[]>> doneAction) {
            this.doneAction = doneAction;
            this.animationsToPlayFile = animationsToPlayFile;
            this.settings = settings;
            this.animFolder = animFolder;

            animLines = File.ReadAllLines(animationsToPlayFile);
			
            Debug.Log($"Loading {animLines.Length} animations from files. If there are a lot, this could take a few seconds...");

            animationSequence = new List<MoshAnimation[]>();

            StartCoroutine(LoadAnimations());
        }

        IEnumerator LoadAnimations() {
            for (int lineIndex = 0; lineIndex < animLines.Length; lineIndex++) {
                Debug.Log($"Loading animation {lineIndex} of {animLines.Length}. This could take a few seconds if there are a lot...");
                string line = animLines[lineIndex];
                MoshAnimation[] allAnimationsInThisLine = GetAnimationsFromLine(line);
                animationSequence.Add(allAnimationsInThisLine);
                
                yield return null;
				
            }
            Debug.Log("Done Loading All Animations.");
            doneAction.Invoke(animationSequence);
        }


        MoshAnimation[] GetAnimationsFromLine(string line) {
            //TODO maybe better way to store list of animationSequence? Needs to be MatLab-friendly for Niko.
            string[] fileNames = line.Split (' '); //Space delimited
            MoshAnimation[] animations = new MoshAnimation[fileNames.Length];
            for (int index = 0; index < fileNames.Length; index++) {
                string filename = fileNames[index];
                string animationFileString = LoadAnimFileAsString(filename);
                animations[index] = new MoshAnimationFromJSON(animationFileString).BuildWithSettings(settings);
            }
            return animations;
        }
		
        string LoadAnimFileAsString(string filename) {
            string animFilePath = Path.Combine(animFolder, filename);
            string animText1 = File.ReadAllText(animFilePath);
            return animText1;
        }
    }
}