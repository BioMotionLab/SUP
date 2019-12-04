using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MoshPlayer.Scripts.BML.FileLoaders;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    public class AnimationLoader : MonoBehaviour {
		
        string       animationsToPlayFile;
        SMPLSettings settings;
        string       animFolder;

        public bool                  DoneLoading;
        public List<List<MoshAnimation>> animationSequence;
        string[]                     animLines;
        Action<List<List<MoshAnimation>>>                       doneAction;


        public void Init(string animationsToPlayFile, SMPLSettings settings, string animFolder, Action<List<List<MoshAnimation>>> doneAction) {
            this.doneAction = doneAction;
            this.animationsToPlayFile = animationsToPlayFile;
            this.settings = settings;
            this.animFolder = animFolder;

            animLines = File.ReadAllLines(animationsToPlayFile);
			
            Debug.Log($"Loading {animLines.Length} animations from files. If there are a lot, this could take a few seconds...");

            animationSequence = new List<List<MoshAnimation>>();

            StartCoroutine(LoadAnimations());
        }

        IEnumerator LoadAnimations() {
            for (int lineIndex = 0; lineIndex < animLines.Length; lineIndex++) {
                StringBuilder log = new StringBuilder();
                string line = animLines[lineIndex];
                List<MoshAnimation> allAnimationsInThisLine = GetAnimationsFromLine(line);
                log.Append($"Loaded animation {lineIndex} of {animLines.Length}");
                if (allAnimationsInThisLine.Count > 0) {
                    animationSequence.Add(allAnimationsInThisLine);
                    log.Append($", containing animations for {allAnimationsInThisLine.Count} characters");
                }
                else {
                    log.Append(", which resulted in no successfully loaded animations. Skipping line.");
                }
                Debug.Log(log);
                yield return null;
            }
            Debug.Log($"Done Loading All Animations. Successfully loaded {animationSequence.Count} of {animLines.Length}.");
            doneAction.Invoke(animationSequence);
        }


        List<MoshAnimation> GetAnimationsFromLine(string line) {
            //TODO maybe better way to store list of animationSequence? Needs to be MatLab-friendly for Niko.
            string[] fileNames = line.Split (' '); //Space delimited
            List<MoshAnimation> animations = new List<MoshAnimation>();
            for (int index = 0; index < fileNames.Length; index++) {
                string filename = fileNames[index];
                string animationFileString;
                try {
                    animationFileString = LoadAnimFileAsString(filename);
                    animations.Add(new MoshAnimationFromJSON(animationFileString).BuildWithSettings(settings));
                }
                catch (FileNotFoundException e) {
                    Debug.LogError($"Trying to load animation file {filename} from folder {animFolder} but could not find it.");
                }
                
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