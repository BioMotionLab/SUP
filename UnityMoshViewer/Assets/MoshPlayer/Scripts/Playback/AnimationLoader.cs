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
		
        Models models;
        string       animFolder;

        [FormerlySerializedAs("animationSequence")]
        public List<List<MoshAnimation>> AnimationSequence;
        string[] animLines;
        Action<List<List<MoshAnimation>>>                       doThisWhenDoneAction;
        PlaybackOptions playbackOptions;


        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public void Init(string animationsToPlayFile, Models models, PlaybackOptions playbackOptions, string animFolder, Action<List<List<MoshAnimation>>> doneAction) {
            doThisWhenDoneAction = doneAction;
            this.models = models;
            this.animFolder = animFolder;
            this.playbackOptions = playbackOptions;

            animLines = File.ReadAllLines(animationsToPlayFile);

            string updateMessage = $"Loading {animLines.Length} animations from files. If there are a lot, this could take a few seconds...";
            Debug.Log(updateMessage);
            PlaybackEventSystem.UpdatePlayerProgress(updateMessage);

            AnimationSequence = new List<List<MoshAnimation>>();

            StartCoroutine(LoadAnimations());
        }
        
        public void Init(string animationFile, Models models, PlaybackOptions playbackOptions, Action<List<List<MoshAnimation>>> doneAction) {
            doThisWhenDoneAction = doneAction;
            this.models = models;
            this.animFolder = Path.GetDirectoryName(animationFile);
            this.playbackOptions = playbackOptions;

            animLines = new string[1];
            animLines[0] = Path.GetFileName(animationFile);

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
                log.Append($"Loaded {lineIndex+1} of {animLines.Length}");
                if (allAnimationsInThisLine.Count > 0) {
                    AnimationSequence.Add(allAnimationsInThisLine);
                    log.Append($" (Model:{allAnimationsInThisLine[0].Data.Model.ModelName}), containing animations for {allAnimationsInThisLine.Count} characters");
                }
                else {
                    log.Append(" [WITH ERRORS]. Skipping line.");
                }

                string finalLog = $"\t...{log}";
                Debug.Log(finalLog);
                PlaybackEventSystem.UpdatePlayerProgress(log.ToString());
                yield return null;
            }

            string updateMessage = $"Done Loading All Animations. Successfully loaded {AnimationSequence.Count} of {animLines.Length}.";
            Debug.Log(updateMessage);
            PlaybackEventSystem.AnimationsDoneLoading();
            PlaybackEventSystem.UpdatePlayerProgress(updateMessage);
            doThisWhenDoneAction.Invoke(AnimationSequence);
        }


        List<MoshAnimation> GetAnimationsFromLine(string line) {
            //TODO maybe better way to store list of animationSequence? Needs to be MatLab-friendly for Niko.
            string[] fileNames = line.Split (' '); //Space delimited
            List<MoshAnimation> animations = new List<MoshAnimation>();
            foreach (string filename in fileNames) {
                try {
                    if (!Directory.Exists(animFolder)) throw new DirectoryNotFoundException(animFolder);
                    string animFilePath = Path.Combine(animFolder, filename);
                    
                    
                    AnimationFileLoader loader;
                    string extension = Path.GetExtension(animFilePath);
                    if (extension == ".json") loader = new AnimationFromJSON(animFilePath, models, playbackOptions);
                    else if (extension == ".h5")
                        loader = new AnimationFromH5(animFilePath, models, playbackOptions);
                    else throw new UnsupportedFileTypeException($"Extension {extension} is unsupported");
                    AnimationData animationData = loader.Data;
                    
                    MoshAnimation loadedAnimation = new MoshAnimation(animationData, playbackOptions, filename);
                    animations.Add(loadedAnimation);
                }
                catch (FileNotFoundException) {
                    Debug.LogError($"Trying to load animation but could not find the file specified. Details below: " +
                                   $"\n\t\tFileName: {filename}" +
                                   $"\n\t\tFolder: {animFolder} ");
                }
                catch (FileMissingFromFolderException) {
                    Debug.LogError("Folder exists, but listed file not found inside it." +
                                   $"\n\t\tFileName: {filename}" +
                                   $"\n\t\tFolder: {animFolder} ");
                }
                catch (Exception e) when 
                    (e is DataReadException || 
                     e is UnsupportedFileTypeException) {
                    
                    Debug.LogError(e.Message +
                                   $"\n\t\tFileName: {filename}" +
                                   $"\n\t\tFolder: {animFolder} ");
                }
                
            }
            return animations;
        }
		
        

      
    }

    public class FileMissingFromFolderException : Exception {

        public FileMissingFromFolderException() {
        }

        public FileMissingFromFolderException(string e) : base(e) {
        }

    }
    
    public class UnsupportedFileTypeException : Exception {

        public UnsupportedFileTypeException() {
        }

        public UnsupportedFileTypeException(string e) : base(e) {
        }

    }
    
    public class DataReadException : Exception {

        public DataReadException() {
        }

        public DataReadException(string e) : base(e) {
        }

    }
}