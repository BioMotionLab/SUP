using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FileLoaders;
using SMPLModel;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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

            List<List<AMASSAnimation>> loadedSequence = await LoadAnimationsAsync();
            
            doneAction.Invoke(loadedSequence);
        }
        
        
        public static async void LoadSamplesAsync(SamplesList samples, Models models , PlaybackSettings playbackSettings,  Action<List<List<AMASSAnimation>>> doneLoading) {
            List<List<AMASSAnimation>> loadedSamples = new List<List<AMASSAnimation>>();
            foreach (SampleGroup sampleGroup in samples.SampleGroups) {
                List<AMASSAnimation> animationsInThisGroup = new List<AMASSAnimation>();
                foreach (AssetReference sample in sampleGroup.samples) {
                    AsyncOperationHandle<TextAsset> handle = sample.LoadAssetAsync<TextAsset>();
                    await handle.Task;
                    if (handle.Status == AsyncOperationStatus.Succeeded) {
                        TextAsset textAsset = handle.Result;
                        AnimationLoadStrategy loadStrategy = new LoadAnimationFromJsonTextAsset(textAsset, models);
                        AnimationData animationData = await LoadDataAsync(loadStrategy);
                        AMASSAnimation loadedAnimation = new AMASSAnimation(animationData, playbackSettings, textAsset.name);
                        animationsInThisGroup.Add(loadedAnimation);
                    }
                    else {
                        Debug.LogError($"Error loading {sample.Asset.name}");
                    }
                    // The task is complete. Be sure to check the Status is successful before storing the Result.
                }
                loadedSamples.Add(animationsInThisGroup);
            }

            doneLoading.Invoke(loadedSamples);
        }

        static async Task<AnimationData> LoadDataAsync(AnimationLoadStrategy loadStrategy) {
            AnimationData loadDataFromSampleJson = await loadStrategy.LoadData();
            return loadDataFromSampleJson;
        }


        async Task<List<List<AMASSAnimation>>> LoadAnimationsAsync() {
            List<List<AMASSAnimation>> animationSequence = new List<List<AMASSAnimation>>();
            
            for (int lineIndex = 0; lineIndex < animationFileReference.Count; lineIndex++) {
                StringBuilder log = new StringBuilder();
                
                string line = animationFileReference.AnimListAsStrings[lineIndex];
                List<AMASSAnimation> allAnimationsInThisLine = await GetAnimationsFromLine(line);
                
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


        async Task<List<AMASSAnimation>> GetAnimationsFromLine(string line) {
            
            string[] fileNames = line.Split (' '); //Space delimited
            List<AMASSAnimation> animations = new List<AMASSAnimation>();
            foreach (string filename in fileNames) {
                try {
                    if (!Directory.Exists(animationFileReference.AnimFolder))
                        throw new DirectoryNotFoundException(animationFileReference.AnimFolder);
                    string animFilePath = Path.Combine(animationFileReference.AnimFolder, filename);


                    AnimationLoadStrategy loadStrategy;
                    string extension = Path.GetExtension(animFilePath);
                    if (extension == ".json") loadStrategy = new LoadAnimationFromJSONFile(animFilePath, models);

                    // BUG else if (extension == ".h5") loadStrategy = new AnimationFromH5(animFilePath, models);
                    else
                        throw new AnimationLoadFromFile.UnsupportedFileTypeException(
                            $"Extension {extension} is unsupported");

                    AnimationData animationData = await LoadDataAsync(loadStrategy);

                    AMASSAnimation loadedAnimation = new AMASSAnimation(animationData, playbackSettings, filename);
                    animations.Add(loadedAnimation);
                }
                catch (FileNotFoundException) {
                    Debug.LogError($"Trying to load animation but could not find the file specified. Details below: " +
                                   $"\n\t\tFileName: {filename}" +
                                   $"\n\t\tFolder: {animationFileReference.AnimFolder} ");
                }
                catch (AnimationLoadFromFile.FileMissingFromFolderException) {
                    Debug.LogError("Folder exists, but listed file not found inside it." +
                                   $"\n\t\tFileName: {filename}" +
                                   $"\n\t\tFolder: {animationFileReference.AnimFolder} ");
                }
                catch (AnimationLoadFromFile.UnsupportedFileTypeException e) {
                    if (e.Message.Contains("h5")) {
                        Debug.LogWarning($"H5 Support is current disabled due to a bug in unity. I'm working on a workaround - Adam. File skipped: {filename} ");
                    }
                    else {
                        Debug.LogError(e.Message +
                                       $"\n\t\tFileName: {filename}" +
                                       $"\n\t\tFolder: {animationFileReference.AnimFolder} ");
                    }
                    
                }
                catch (AnimationLoadStrategy.DataReadException e ) {
                    Debug.LogError(e.Message +
                                   $"\n\t\tFileName: {filename}" +
                                   $"\n\t\tFolder: {animationFileReference.AnimFolder} ");
                    
                }
                
            }
            
            return animations;
            
            
            
            
        }


        
    }

    
}