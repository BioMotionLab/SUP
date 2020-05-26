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
        
        public List<List<MoshAnimation>> AnimationSequence;
        string[] animListAsStrings;
        Action<List<List<MoshAnimation>>>                       doneAction;
        PlaybackSettings playbackSettings;


        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public void Init(string animationsListFile, string animFolder, Models models, PlaybackSettings playbackSettings, Action<List<List<MoshAnimation>>> doneAction) {
            if (!File.Exists(animationsListFile)) throw new IOException($"Can't find List of Animations file {animationsListFile}");
            
            this.doneAction = doneAction;
            this.models = models;
            this.animFolder = animFolder;
            this.playbackSettings = playbackSettings;

            animListAsStrings = File.ReadAllLines(animationsListFile);

            string updateMessage = $"Loading {animListAsStrings.Length} animations from files. If there are a lot, this could take a few seconds...";
            Debug.Log(updateMessage);
            PlaybackEventSystem.UpdatePlayerProgress(updateMessage);

            AnimationSequence = new List<List<MoshAnimation>>();

            StartCoroutine(LoadAnimations());
        }
        
        public void Init(string animationFile, Models models, PlaybackSettings playbackSettings, Action<List<List<MoshAnimation>>> doneAction) {
            if (!File.Exists(animationFile)) throw new IOException($"Can't find Animation file {animationFile}");
            
            this.doneAction = doneAction;
            this.models = models;
            this.animFolder = Path.GetDirectoryName(animationFile);
            this.playbackSettings = playbackSettings;

            animListAsStrings = new string[1];
            animListAsStrings[0] = Path.GetFileName(animationFile);

            string updateMessage = $"Loading {animListAsStrings.Length} animations from files. If there are a lot, this could take a few seconds...";
            Debug.Log(updateMessage);
            PlaybackEventSystem.UpdatePlayerProgress(updateMessage);

            AnimationSequence = new List<List<MoshAnimation>>();

            StartCoroutine(LoadAnimations());
        }

        IEnumerator LoadAnimations() {
            for (int lineIndex = 0; lineIndex < animListAsStrings.Length; lineIndex++) {
                StringBuilder log = new StringBuilder();
                string line = animListAsStrings[lineIndex];
                List<MoshAnimation> allAnimationsInThisLine = GetAnimationsFromLine(line);
                log.Append($"Loaded {lineIndex+1} of {animListAsStrings.Length}");
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

            string updateMessage = $"Done Loading All Animations. Successfully loaded {AnimationSequence.Count} of {animListAsStrings.Length}.";
            Debug.Log(updateMessage);
            PlaybackEventSystem.AnimationsDoneLoading();
            PlaybackEventSystem.UpdatePlayerProgress(updateMessage);
            doneAction.Invoke(AnimationSequence);
        }


        List<MoshAnimation> GetAnimationsFromLine(string line) {
            //TODO maybe better way to store list of animationSequence? Needs to be MatLab-friendly for Niko.
            string[] fileNames = line.Split (' '); //Space delimited
            List<MoshAnimation> animations = new List<MoshAnimation>();
            foreach (string filename in fileNames) {
                try {
                    if (!Directory.Exists(animFolder)) throw new DirectoryNotFoundException(animFolder);
                    string animFilePath = Path.Combine(animFolder, filename);
                    
                    
                    AnimationLoadStrategy loadStrategy;
                    string extension = Path.GetExtension(animFilePath);
                    if (extension == ".json") loadStrategy = new AnimationFromJSON(animFilePath, models, playbackSettings);
                    else if (extension == ".h5")
                        loadStrategy = new AnimationFromH5(animFilePath, models, playbackSettings);
                    else throw new AnimationLoadStrategy.UnsupportedFileTypeException($"Extension {extension} is unsupported");
                    AnimationData animationData = loadStrategy.Data;
                    
                    MoshAnimation loadedAnimation = new MoshAnimation(animationData, playbackSettings, filename);
                    animations.Add(loadedAnimation);
                }
                catch (FileNotFoundException) {
                    Debug.LogError($"Trying to load animation but could not find the file specified. Details below: " +
                                   $"\n\t\tFileName: {filename}" +
                                   $"\n\t\tFolder: {animFolder} ");
                }
                catch (AnimationLoadStrategy.FileMissingFromFolderException) {
                    Debug.LogError("Folder exists, but listed file not found inside it." +
                                   $"\n\t\tFileName: {filename}" +
                                   $"\n\t\tFolder: {animFolder} ");
                }
                catch (Exception e) when 
                    (e is AnimationLoadStrategy.DataReadException || 
                     e is AnimationLoadStrategy.UnsupportedFileTypeException) {
                    
                    Debug.LogError(e.Message +
                                   $"\n\t\tFileName: {filename}" +
                                   $"\n\t\tFolder: {animFolder} ");
                }
                
            }
            return animations;
            
            
            
            
        }
	
      
    }

    
}