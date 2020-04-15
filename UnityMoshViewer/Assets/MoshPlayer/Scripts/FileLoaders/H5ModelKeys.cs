using System;

namespace MoshPlayer.Scripts.FileLoaders {
    [Serializable]
    public class H5ModelKeys {
        public string Gender       = "gender";
        public string Male         = "male";
        public string Female       = "female";
        public string FPS          = "mocap_framerate";
        public string Translations = "trans";
        public string Poses        = "poses";
        public string Betas        = "betas";
    }
}