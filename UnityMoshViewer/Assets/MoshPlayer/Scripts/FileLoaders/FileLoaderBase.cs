using System;
using System.IO;
using MoshPlayer.Scripts.Playback;
using MoshPlayer.Scripts.SMPLModel;
using UnityEngine;

namespace MoshPlayer.Scripts.FileLoaders {
    public abstract class FileLoaderBase : AnimationFileLoader{
        
        protected readonly AnimationData animationData;
        protected string filePath;
        public AnimationData Data => animationData;

        readonly Models possibleModels;
        protected readonly PlaybackOptions playbackOptions;

        protected FileLoaderBase(string filePath, Models possibleModels, PlaybackOptions playbackOptions) {
            if (possibleModels == null) throw new NullReferenceException("No Models specified");
            
            this.filePath = filePath ?? throw new NullReferenceException("Tried to instantiate Animation JSON with null TextAsset");
             
            if (!File.Exists(filePath)) {
                Debug.LogError($"No File at {filePath}");
                throw new FileMissingFromFolderException();
            }
            
            this.possibleModels = possibleModels;
            this.playbackOptions = playbackOptions;
            
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
                throw new DataReadException("Support for SMPL is currently untested, so I disabled it. If you need this functionality contact me and I can get it working -Adam");
        }

        protected abstract void LoadData();
        protected abstract bool IsMatchingModel(ModelDefinition model);

        protected abstract void FormatData();
    }
}