namespace Playback {
    public static class Format {
        
       const string SUPPrefix = "[SUP]";
        public static string Log(string message) {
            return $"{SUPPrefix} {message}";
        }

        public static string Warning(string message) { 
            return FormatWarning($"{SUPPrefix} {message}");
        }
        public static string Error(string message) { 
            return FormatError($"{SUPPrefix} {message}");
        }
        
        static string FormatError(string message) {
            return $"<color=red>{message}</color>";
        }
        
        static string FormatWarning(string message) {
            return $"<color=orange>{message}</color>";
        }
    }
}