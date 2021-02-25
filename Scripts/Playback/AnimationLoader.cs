using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FileLoaders;
using SMPLModel;
using UnityEngine;

namespace Playback {
    public class AnimationLoader : MonoBehaviour {
		
        Models models;
        
        AnimationFileReference animationFileReference;
        
        PlaybackSettings playbackSettings;

        public async void LoadAsync(AnimationFileReference animationsFileReference, Models models, PlaybackSettings playbackSettings, Action<List<List<AMASSAnimation>>> doneAction) {
           
            this.models = models;
            this.playbackSettings = playbackSettings;
            this.animationFileReference = animationsFileReference;
            
            string updateMessage = $"Loading {animationsFileReference.Count} animations from files. If there are a lot, this could take a few seconds...";
            Debug.Log(updateMessage);
            PlaybackEventSystem.UpdatePlayerProgress(updateMessage);

            List<List<AMASSAnimation>> loadedSequence = await Task.Run(LoadAnimationsAsync);
            
            doneAction.Invoke(loadedSequence);
        }
        
        List<List<AMASSAnimation>> LoadAnimationsAsync() {
            List<List<AMASSAnimation>> animationSequence = new List<List<AMASSAnimation>>();
            
            for (int lineIndex = 0; lineIndex < animationFileReference.Count; lineIndex++) {
                StringBuilder log = new StringBuilder();
                
                string line = animationFileReference.AnimListAsStrings[lineIndex];
                List<AMASSAnimation> allAnimationsInThisLine = GetAnimationsFromLine(line);
                
                log.Append($"Loaded {lineIndex+1} of {animationFileReference.AnimListAsStrings.Length}");

                if (allAnimationsInThisLine.Count == 0) {
                    log.Append(" [WITH ERRORS]. Skipping line.");
                    continue;
                }
                
                animationSequence.Add(allAnimationsInThisLine);
                log.Append($" (Model:{allAnimationsInThisLine[0].Data.Model.ModelName}), containing animations for {allAnimationsInThisLine.Count} characters");

                string finalLog = $"\t...{log}";
                Debug.Log(finalLog);
                PlaybackEventSystem.UpdatePlayerProgress(log.ToString());
            }

            string updateMessage = $"Done Loading All Animations. Successfully loaded {animationSequence.Count} of {animationFileReference.AnimListAsStrings.Length}.";
            PlaybackEventSystem.UpdatePlayerProgress(updateMessage);
            Debug.Log(updateMessage);
            return (animationSequence);
            
            
        } 


        List<AMASSAnimation> GetAnimationsFromLine(string line) {
            
            string[] fileNames = line.Split (' '); //Space delimited
            List<AMASSAnimation> animations = new List<AMASSAnimation>();
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
                    
                    AMASSAnimation loadedAnimation = new AMASSAnimation(animationData, playbackSettings, filename);
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