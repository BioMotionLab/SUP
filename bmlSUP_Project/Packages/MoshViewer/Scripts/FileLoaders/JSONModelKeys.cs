using System;

// ReSharper disable InconsistentNaming

namespace FileLoaders {
    
    [Serializable]
    public class JSONModelKeys {
        public string Gender       = "gender";
        public string Male         = "male";
        public string Female       = "female";
        public string FPS          = "fps";
        public string Translations = "trans";
        public string Poses        = "poses";
        public string Betas        = "betas";
    }
}