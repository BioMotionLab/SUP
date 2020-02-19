using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using MoshPlayer.Scripts.FileLoaders;
using MoshPlayer.Scripts.SMPLModel;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.Playback {
    public class AnimationLoader : MonoBehaviour {
		
        SettingsMain settingsMain;
        string       animFolder;

        public bool                  DoneLoading;
        [FormerlySerializedAs("animationSequence")]
        public List<List<MoshAnimation>> AnimationSequence;
        string[]                     animLines;
        Action<List<List<MoshAnimation>>>                       doThisWhenDoneAction;
        PlaybackOptions playbackOptions;


        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public void Init(string animationsToPlayFile, SettingsMain settingsMain, PlaybackOptions playbackOptions, string animFolder, Action<List<List<MoshAnimation>>> doneAction) {
            doThisWhenDoneAction = doneAction;
            this.settingsMain = settingsMain;
            this.animFolder = animFolder;
            this.playbackOptions = playbackOptions;

            animLines = File.ReadAllLines(animationsToPlayFile);

            string updateMessage = $"Loading {animLines.Length} animations from files. If there are a lot, this could take a few seconds...";
            Debug.Log(updateMessage);
            PlaybackEventSystem.UpdatePlayerProgress(updateMessage);

            AnimationSequence = new List<List<MoshAnimation>>();

            StartCoroutine(LoadAnimations());
        }

        IEnumerator LoadAnimations() {
            for (int lineIndex = 0; lineIndex < animLines.Length; lineIndex++) {
                StringBuilder log = new StringBuilder();
                string line = animLines[lineIndex];
                List<MoshAnimation> allAnimationsInThisLine = GetAnimationsFromLine(line);
                log.Append($"Loaded {lineIndex} of {animLines.Length} (Model:{allAnimationsInThisLine[0].Model.ModelName})");
                if (allAnimationsInThisLine.Count > 0) {
                    AnimationSequence.Add(allAnimationsInThisLine);
                    log.Append($", containing animations for {allAnimationsInThisLine.Count} characters");
                }
                else {
                    log.Append(", which resulted in no successfully loaded animations. Skipping line.");
                }

                string finalLog = $"\t...{log}";
                Debug.Log(finalLog);
                PlaybackEventSystem.UpdatePlayerProgress(log.ToString());
                yield return null;
            }

            string updateMessage = $"Done Loading All Animations. Successfully loaded {AnimationSequence.Count} of {animLines.Length}.";
            Debug.Log(updateMessage);
            PlaybackEventSystem.UpdatePlayerProgress(updateMessage);
            doThisWhenDoneAction.Invoke(AnimationSequence);
        }


        List<MoshAnimation> GetAnimationsFromLine(string line) {
            //TODO maybe better way to store list of animationSequence? Needs to be MatLab-friendly for Niko.
            string[] fileNames = line.Split (' '); //Space delimited
            List<MoshAnimation> animations = new List<MoshAnimation>();
            foreach (string filename in fileNames) {
                try {
                    string animationFileString = LoadAnimFileAsString(filename);
                    MoshAnimation loadedAnimation =
                        new MoshAnimationFromJSON(animationFileString, settingsMain, playbackOptions, filename)
                            .BuildWithSettings();
                    animations.Add(loadedAnimation);
                }
                catch (FileNotFoundException) {
                    Debug.LogError($"Trying to load animation but could not find the file specified. Details below: " +
                                   $"\n\t\tFileName: {filename}" +
                                   $"\n\t\tFolder: {animFolder} ");
                }
                catch (FileMissingFromFolderException) {
                    Debug.Log("Folder exists, but listed file not found inside it." +
                        $"\n\t\tFileName: {filename}" +
                        $"\n\t\tFolder: {animFolder} ");
                }
            }
            return animations;
        }
		
        string LoadAnimFileAsString(string filename) {
            string animFilePath = Path.Combine(animFolder, filename);
            
            if (Directory.Exists(animFolder)) {
                if (!File.Exists(animFilePath)) {
                    throw new FileMissingFromFolderException();
                }
            }

            //Debug.Log(animFilePath);
            string animText1 = File.ReadAllText(animFilePath);
            return animText1;
        }
    }

    public class FileMissingFromFolderException : Exception {

        public FileMissingFromFolderException() {
        }

        public FileMissingFromFolderException(string e) : base(e) {
        }

    }
}