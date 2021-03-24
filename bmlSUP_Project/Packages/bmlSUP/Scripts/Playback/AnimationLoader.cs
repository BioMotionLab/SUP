using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FileLoaders;
using JetBrains.Annotations;
using Settings;
using SMPLModel;
using UnityEngine;

namespace Playback {
    public static class AnimationLoader {

        public static async void LoadAsync(AnimationFileReference animationsFileReference, Models models, PlaybackSettings playbackSettings, Action<List<List<AMASSAnimation>>> doneAction) {
           
            string updateMessage = $"Loading {animationsFileReference.Count} animations from files. If there are a lot, this could take a few seconds...";
            Debug.Log(updateMessage);
            PlaybackEventSystem.UpdatePlayerProgress(updateMessage);

            List<List<AMASSAnimation>> loadedSequence = await LoadAnimationsAsync(animationsFileReference, models, playbackSettings);
            
            doneAction.Invoke(loadedSequence);
        }
        
        [PublicAPI]
        public static void LoadFromAnimationListAssetAsync(AnimationListAsset animationListAsset, Action<List<List<AMASSAnimation>>> doneLoading) {
            LoadFromAnimationListAssetAsync(animationListAsset, animationListAsset.Models, animationListAsset.PlaybackSettings, doneLoading);
        }

       public static async void LoadFromAnimationListAssetAsync(AnimationListAsset animationListAsset, Models models, PlaybackSettings playbackSettings, Action<List<List<AMASSAnimation>>> doneLoading) {
            List<List<AMASSAnimation>> loadedSamples = new List<List<AMASSAnimation>>();
            foreach (AnimationAssetGroup sampleGroup in animationListAsset.AnimationAssetGroups) {
                List<AMASSAnimation> animationsInThisGroup = new List<AMASSAnimation>();

                string loadLog = "Loaded: ";
                foreach (TextAsset textAsset in sampleGroup.assets) {

                    if (textAsset == null) continue;
                    
                    AnimationLoadStrategy loadStrategy = new LoadAnimationFromJsonTextAsset(textAsset, models);
                    AnimationData animationData = await LoadDataAsync(loadStrategy);
                    AMASSAnimation loadedAnimation = new AMASSAnimation(animationData, playbackSettings, textAsset.name);
                    animationsInThisGroup.Add(loadedAnimation);
                    loadLog += $"{loadedAnimation.AnimationName} ";
                    
                }
                loadedSamples.Add(animationsInThisGroup);


                Debug.Log(Format.Log(loadLog));
                PlaybackEventSystem.UpdatePlayerProgress(loadLog);
            }

            doneLoading.Invoke(loadedSamples);
        }

        static void DisplayPackageImportErrorMessage() {
            Debug.LogError(Format.Error("Error loading animationListAsset. "));
            Debug.LogError(Format.Error("\tProbable cause:"));
            Debug.LogError(Format.Error("\t \tThe \"Addressables\" package was loaded AFTER importing SUP."));
            Debug.LogError(Format.Error("\t \tThis is a unity editor limitation."));
            Debug.LogError(Format.Error("\tProbable solution:"));
            Debug.LogError(
                Format.Error("\t \t1) Ensure the \"Addressables\" package is installed in your project."));
            Debug.LogError(
                Format.Error("\t \t2) Go to Window Menu > Asset Management > Addressables > Groups, Then click the button to generate a settings file."));
            Debug.LogError(
                Format.Error("\t \t3) Reimport SUP (Go to Packages/bmlSUP, right click on folder, and click reimport)."));
        }

        static async Task<AnimationData> LoadDataAsync(AnimationLoadStrategy loadStrategy) {
            AnimationData loadDataFromSampleJson = await loadStrategy.LoadData();
            return loadDataFromSampleJson;
        }


        static async Task<List<List<AMASSAnimation>>> LoadAnimationsAsync(AnimationFileReference animationsFileReference, Models models, PlaybackSettings playbackSettings) {
            List<List<AMASSAnimation>> animationSequence = new List<List<AMASSAnimation>>();
            
            for (int lineIndex = 0; lineIndex < animationsFileReference.Count; lineIndex++) {
                StringBuilder log = new StringBuilder();
                
                string line = animationsFileReference.AnimListAsStrings[lineIndex];
                List<AMASSAnimation> allAnimationsInThisLine = await GetAnimationsFromLine(line, animationsFileReference, models, playbackSettings);
                
                log.Append($"Loaded {lineIndex+1} of {animationsFileReference.AnimListAsStrings.Length}");

                if (allAnimationsInThisLine.Count == 0) {
                    log.Append(" [WITH ERRORS]. Skipping line.");
                    continue;
                }
                
                animationSequence.Add(allAnimationsInThisLine);
                log.Append($" (Model:{allAnimationsInThisLine[0].Data.Model.ModelName}), containing animations for {allAnimationsInThisLine.Count} characters");

                string finalLog = $"\t...{log}";
                Debug.Log(Format.Log(finalLog));
                PlaybackEventSystem.UpdatePlayerProgress(log.ToString());
            }

            string updateMessage = $"Done Loading All Animations. Successfully loaded {animationSequence.Count} of {animationsFileReference.AnimListAsStrings.Length}.";
            PlaybackEventSystem.UpdatePlayerProgress(updateMessage);
            Debug.Log(Format.Log(updateMessage));
            return (animationSequence);
            
            
        }
        

        static async Task<List<AMASSAnimation>> GetAnimationsFromLine(string line, AnimationFileReference animationsFileReference, Models models, PlaybackSettings playbackSettings) {
            
            string[] fileNames = line.Split (' '); //Space delimited
            List<AMASSAnimation> animations = new List<AMASSAnimation>();
            foreach (string filename in fileNames) {
                try {
                    if (!Directory.Exists(animationsFileReference.AnimFolder))
                        throw new DirectoryNotFoundException(animationsFileReference.AnimFolder);
                    string animFilePath = Path.Combine(animationsFileReference.AnimFolder, filename);


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
                    Debug.LogError(Format.Error($"Trying to load animationListAsset but could not find the file specified. Details below: " +
                                                $"\n\t\tFileName: {filename}" +
                                                $"\n\t\tFolder: {animationsFileReference.AnimFolder} "));
                }
                catch (AnimationLoadFromFile.FileMissingFromFolderException) {
                    Debug.LogError(Format.Error("Folder exists, but listed file not found inside it." +
                                                $"\n\t\tFileName: {filename}" +
                                                $"\n\t\tFolder: {animationsFileReference.AnimFolder} "));
                }
                catch (AnimationLoadFromFile.UnsupportedFileTypeException e) {
                    if (e.Message.Contains("h5")) {
                        Debug.LogWarning(Format.Warning($"H5 Support is current disabled due to a bug in unity. I'm working on a workaround - Adam. File skipped: {filename} "));
                    }
                    else {
                        Debug.LogError(Format.Error(e.Message +
                                                    $"\n\t\tFileName: {filename}" +
                                                    $"\n\t\tFolder: {animationsFileReference.AnimFolder} "));
                    }
                    
                }
                catch (AnimationLoadStrategy.DataReadException e ) {
                    Debug.LogError(Format.Error(e.Message +
                                                $"\n\t\tFileName: {filename}" +
                                                $"\n\t\tFolder: {animationsFileReference.AnimFolder} "));
                    
                }
                
            }
            
            return animations;
            
            
            
            
        }


        
    }
}