using System;
using System.IO;
using SMPLModel;

namespace FileLoaders {
    public abstract class AnimationLoadFromFile : AnimationLoadStrategy {
        
        protected string filePath;
        protected AnimationLoadFromFile(string filePath, Models possibleModels) : base(possibleModels) {
            
            this.filePath = filePath ?? throw new NullReferenceException("Tried to instantiate Animation JSON with null TextAsset");
            if (!File.Exists(filePath)) throw new FileMissingFromFolderException($"No File at {filePath}");

            
        }
        
        
        public class FileMissingFromFolderException : Exception {

            public FileMissingFromFolderException() {
            }

            public FileMissingFromFolderException(string e) : base(e) {
            }

        }

        public class UnsupportedFileTypeException : Exception {

            public UnsupportedFileTypeException() {
            }

            public UnsupportedFileTypeException(string e) : base(e) {
            }

        }
    }
}