using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Playback {
    public class AnimationFileReference {


        string animFolder;
        string[] animListAsStrings;
        public int Count => animListAsStrings.Length;
        public string[] AnimListAsStrings => animListAsStrings;
        public string AnimFolder => animFolder;

        public AnimationFileReference(string animFolder, string animationsListFile = null) {
            this.animFolder = animFolder;
            
            if (animationsListFile != null) animListAsStrings = LoadFilesInFolderFromList(animationsListFile);
            else animListAsStrings = LoadAllFilesInFolder();
        }

        string[] LoadFilesInFolderFromList(string animationsListFile) {
            if (File.Exists(animationsListFile)) return File.ReadAllLines(animationsListFile);
            else {
                Debug.LogError($"Animation list file path specified, but could not find file at {animationsListFile}");
                return new string[] {};
            }
        }

        string[] LoadAllFilesInFolder() {
            string[] extensions = {"*.json", "*.h5"};

            List<string> foundFiles = new List<string>();
            foreach (string fileExtension in extensions) {
                string[] filesWithExtension = Directory.GetFiles(animFolder, fileExtension, SearchOption.AllDirectories);
                foreach (string file in filesWithExtension) {
                    string justFileName = Path.GetFileName(file);
                    foundFiles.Add(justFileName);
                }
            }
            return foundFiles.ToArray();
        }

        public AnimationFileReference(string animationFile) {
            if (!File.Exists(animationFile)) throw new IOException($"Can't find List of Animations file {animationFile}");
            animListAsStrings = new string[1];
            animListAsStrings[0] = Path.GetFileName(animationFile);
            this.animFolder = Path.GetDirectoryName(animationFile);
        }

        
    }
}