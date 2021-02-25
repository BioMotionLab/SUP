using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Playback {
    public class AnimationFileReference {


        string animFolder;
        string[] animListAsStrings;
        public int Count => animListAsStrings.Length;
        public string[] AnimListAsStrings => animListAsStrings;
        public string AnimFolder => animFolder;

        public AnimationFileReference(string animationsListFile, string animFolder) {
            if (!Directory.Exists(animFolder)) Debug.LogError($"Directory not found:  {animFolder}");
            
            if (animationsListFile != null && File.Exists(animationsListFile)) {
                animListAsStrings = File.ReadAllLines(animationsListFile);
            }
            else {
                Debug.LogWarning($"No animationListFile provided, playing all .h5 and .json files in folder  {animFolder}");
                List<string> ext = new List<string> { "*.h5", "*.json"};
                List<string> foundFiles = new List<string>();
                foreach(string fileExtension in ext) {
                    string[] files = Directory.GetFiles(animFolder, fileExtension, SearchOption.AllDirectories);
                    Debug.Log($"Found {files.Length} with extension {fileExtension}");
                    foreach(string file in files){
                        foundFiles.Add(file);
                    }
                }
                Debug.Log($"file count {foundFiles.Count}");
                animListAsStrings = foundFiles.ToArray();
            }
            this.animFolder = animFolder;
        }

        public AnimationFileReference(string animationFile) {
            if (!File.Exists(animationFile)) throw new IOException($"Can't find List of Animations file {animationFile}");
            animListAsStrings = new string[1];
            animListAsStrings[0] = Path.GetFileName(animationFile);
            this.animFolder = Path.GetDirectoryName(animationFile);
        }

        
    }
}