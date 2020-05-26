using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MoshPlayer.Scripts.FileLoaders;
using MoshPlayer.Scripts.SMPLModel;
using UnityEngine;


namespace MoshPlayer.Scripts.Playback {
    public class AnimationLoader : MonoBehaviour {
		
        Models models;
        public List<List<MoshAnimation>> AnimationSequence;

        AnimationFileReference animationFileReference;
        
        Action<List<List<MoshAnimation>>> doneAction;
        PlaybackSettings playbackSettings;
        
        public void Init(AnimationFileReference animationsFileReference, Models models, PlaybackSettings playbackSettings, Action<List<List<MoshAnimation>>> doneAction) {
            this.doneAction = doneAction;
            this.models = models;
            this.playbackSettings = playbackSettings;
            this.animationFileReference = animationsFileReference;
            
            string updateMessage = $"Loading {animationsFileReference.Count} animations from files. If there are a lot, this could take a few seconds...";
            Debug.Log(updateMessage);
            PlaybackEventSystem.UpdatePlayerProgress(updateMessage);

            AnimationSequence = new List<List<MoshAnimation>>();

            StartCoroutine(LoadAnimations());
        }

        IEnumerator LoadAnimations() {
            for (int lineIndex = 0; lineIndex < animationFileReference.Count; lineIndex++) {
                StringBuilder log = new StringBuilder();
                string line = animationFileReference.AnimListAsStrings[lineIndex];
                List<MoshAnimation> allAnimationsInThisLine = GetAnimationsFromLine(line);
                log.Append($"Loaded {lineIndex+1} of {animationFileReference.AnimListAsStrings.Length}");
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

            string updateMessage = $"Done Loading All Animations. Successfully loaded {AnimationSequence.Count} of {animationFileReference.AnimListAsStrings.Length}.";
            Debug.Log(updateMessage);
            PlaybackEventSystem.AnimationsDoneLoading();
            PlaybackEventSystem.UpdatePlayerProgress(updateMessage);
            doneAction.Invoke(AnimationSequence);
        }


        List<MoshAnimation> GetAnimationsFromLine(string line) {
            
            string[] fileNames = line.Split (' '); //Space delimited
            List<MoshAnimation> animations = new List<MoshAnimation>();
            foreach (string filename in fileNames) {
                try {
                    if (!Directory.Exists(animationFileReference.AnimFolder)) throw new DirectoryNotFoundException(animationFileReference.AnimFolder);
                    string animFilePath = Path.Combine(animationFileReference.AnimFolder, filename);
                    
                    
                    AnimationLoadStrategy loadStrategy;
                    string extension = Path.GetExtension(animFilePath);
                    if (extension == ".json") loadStrategy = new AnimationFromJSON(animFilePath, models);
                    else if (extension == ".h5")
                        loadStrategy = new AnimationFromH5(animFilePath, models);
                    else throw new AnimationLoadStrategy.UnsupportedFileTypeException($"Extension {extension} is unsupported");
                    AnimationData animationData = loadStrategy.Data;
                    
                    MoshAnimation loadedAnimation = new MoshAnimation(animationData, playbackSettings, filename);
                    animations.Add(loadedAnimation);
                }
                catch (FileNotFoundException) {
                    Debug.LogError($"Trying to load animation but could not find the file specified. Details below: " +
                                   $"\n\t\tFileName: {filename}" +
                                   $"\n\t\tFolder: {animationFileReference.AnimFolder} ");
                }
                catch (AnimationLoadStrategy.FileMissingFromFolderException) {
                    Debug.LogError("Folder exists, but listed file not found inside it." +
                                   $"\n\t\tFileName: {filename}" +
                                   $"\n\t\tFolder: {animationFileReference.AnimFolder} ");
                }
                catch (Exception e) when 
                    (e is AnimationLoadStrategy.DataReadException || 
                     e is AnimationLoadStrategy.UnsupportedFileTypeException) {
                    
                    Debug.LogError(e.Message +
                                   $"\n\t\tFileName: {filename}" +
                                   $"\n\t\tFolder: {animationFileReference.AnimFolder} ");
                }
                
            }
            return animations;
            
            
            
            
        }
	
      
    }

    
}