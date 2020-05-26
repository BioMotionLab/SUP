using System;
using System.IO;
using MoshPlayer.Scripts.Playback;
using MoshPlayer.Scripts.SMPLModel;
using UnityEngine;

namespace MoshPlayer.Scripts.FileLoaders {
    public abstract class AnimationLoadStrategy {
        
        protected readonly AnimationData animationData;
        protected string filePath;
        public AnimationData Data => animationData;

        readonly Models possibleModels;
        protected readonly PlaybackSettings PlaybackSettings;

        protected AnimationLoadStrategy(string filePath, Models possibleModels, PlaybackSettings playbackSettings) {
            this.filePath = filePath;
            this.possibleModels = possibleModels;
            this.PlaybackSettings = playbackSettings;    
            
            CheckSetupErrors();
            
            animationData = new AnimationData();
            Init();
        }

        void Init() {
            LoadData();
            FindCorrectModel();
            FormatData();
        }

        void FindCorrectModel() {
            foreach (ModelDefinition model in possibleModels.ModelDefinitions) {
                if (!IsMatchingModel(model)) continue;
                animationData.Model = model;
                //Debug.Log($"Matched model {matchedModel.ModelName}");
                break;
            }
            
            if (animationData.Model == null) 
                throw new NullReferenceException("Could not match animation to a model");
            
            if (animationData.Model.name == "SMPL") 
                throw new DataReadException("Loaded data appears to be from older SMPL. SMPL is currently untested, so I disabled it. If you need this functionality contact me and I can get it working -Adam");
        }

        protected abstract void LoadData();

        protected abstract bool IsMatchingModel(ModelDefinition model);

        protected abstract void FormatData();


        void CheckSetupErrors() {
            if (possibleModels == null) throw new NullReferenceException("No Models specified");
            this.filePath =
                filePath ?? throw new NullReferenceException("Tried to instantiate Animation JSON with null TextAsset");
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

        public class DataReadException : Exception {

            public DataReadException() {
            }

            public DataReadException(string e) : base(e) {
            }

        }
    }
}