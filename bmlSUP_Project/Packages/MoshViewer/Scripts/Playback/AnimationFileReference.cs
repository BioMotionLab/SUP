using System.IO;

namespace Playback {
    public class AnimationFileReference {


        string animFolder;
        string[] animListAsStrings;
        public int Count => animListAsStrings.Length;
        public string[] AnimListAsStrings => animListAsStrings;
        public string AnimFolder => animFolder;

        public AnimationFileReference(string animationsListFile, string animFolder) {
            if (!File.Exists(animationsListFile)) throw new IOException($"Can't find List of Animations file {animationsListFile}");
            animListAsStrings = File.ReadAllLines(animationsListFile);
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