using System;
using System.IO;
using MoshPlayer.Scripts.Playback;
using MoshPlayer.Scripts.SMPLModel;
using MoshPlayer.Scripts.ThirdParty.SimpleJSON;
using MoshPlayer.Scripts.Utilities;
using UnityEngine;

namespace MoshPlayer.Scripts.FileLoaders {
    /// <summary>
    ///  This class can serve as a guide for extending BMLMoShAnimation to other 
    /// data formats in future. 
    /// </summary>
    public class AnimationFromJSON : FileLoaderBase {
        JSONNode jsonNode;

        public AnimationFromJSON(string filePath, Models possibleModels, PlaybackOptions playbackOptions): 
            base(filePath, possibleModels, playbackOptions) { }

        protected override void LoadData() {
            // AB: This is where the load speed bottleneck is. It seems to have trouble parsing even fairly small ~1MB files (~100ms or more).
            // Once parsed it seems to be very fast to load (< 5 ms)
            // According to Google, SimpleJSON is the fastest parser... so perhaps just a limitation of the way the data is stored. 
            
            string jsonString = File.ReadAllText(filePath);
            jsonNode = JSON.Parse(jsonString);
        }


        protected override void FormatData() {
            JSONNode genderNode = jsonNode[animationData.Model.JsonKeys.Gender];
            JSONNode betasNode = jsonNode[animationData.Model.JsonKeys.Betas];
            JSONNode fpsNode = jsonNode[animationData.Model.JsonKeys.FPS];
            JSONNode transNode = jsonNode[animationData.Model.JsonKeys.Translations];
            JSONNode posesNode = jsonNode[animationData.Model.JsonKeys.Poses];

            LoadGender(genderNode);
            //Debug.Log($"Gender: {gender}");
            LoadFPS(fpsNode);
            //Debug.Log($"fps: {fps}");
            LoadBetas(betasNode);
            if (animationData.Fps == 0) {
                Debug.LogWarning($"No fps specified, defaulting to {playbackOptions.FallbackFPS} fps");
                animationData.Fps = playbackOptions.FallbackFPS;
            }
            
            //DebugArray("betas", betas.ToList());
            
            LoadTranslationsAndPosesFromJoints(transNode, posesNode);
            //DebugArray("trans" , translations.ToList());
            //DebugArray("poses" , poses.ToList());
        }

        protected override bool IsMatchingModel(ModelDefinition model) {
            JSONNode betasNode = jsonNode[model.JsonKeys.Betas];
            bool modelMatch = betasNode.Count == model.BodyShapeBetaCount;
            return modelMatch;
        }


        void LoadTranslationsAndPosesFromJoints(JSONNode transNode, JSONNode posesNode) {
            
            animationData.FrameCount = transNode.Count;
            
            animationData.Translations = new Vector3[animationData.FrameCount];
            animationData.Poses = new Quaternion[animationData.FrameCount, animationData.Model.JointCount];
            
            for (int frameIndex = 0; frameIndex < animationData.FrameCount; frameIndex++) {
                LoadTranslationFromJoint(transNode, frameIndex);
                for (int jointIndex = 0; jointIndex < animationData.Model.JointCount; jointIndex++) {
                    LoadPosesFromJoint(posesNode, frameIndex, jointIndex);
                }
            }
        }

        void LoadTranslationFromJoint(JSONNode transNode, int frameIndex) {
            JSONNode thisTranslation = transNode[frameIndex];
            Vector3 translationInMayaCoords = new Vector3(thisTranslation[0], thisTranslation[1], thisTranslation[2]);
            Vector3 translationInUnityCoords = translationInMayaCoords.ConvertTranslationFromMayaToUnity();
            animationData.Translations[frameIndex] = translationInUnityCoords;
        }

        void LoadPosesFromJoint(JSONNode posesNode, int frameIndex, int jointIndex) {
            JSONNode thisPoseJson = posesNode[frameIndex][jointIndex];
            Quaternion poseInMayaCoords = new Quaternion(thisPoseJson[0], thisPoseJson[1], thisPoseJson[2], thisPoseJson[3]);
            Quaternion poseInUnityCoords = poseInMayaCoords.ToLeftHanded();
            animationData.Poses[frameIndex, jointIndex] = poseInUnityCoords;
        }


        void LoadBetas(JSONNode betasNode) {
            animationData. Betas = new float[animationData.Model.BodyShapeBetaCount];
            for (int i = 0; i < animationData.Model.BodyShapeBetaCount; i++) {
                animationData.Betas[i] = betasNode[i];
            }
        }

        void LoadFPS(JSONNode fpsNode) {
            
            if (fpsNode.IsNull) throw new NullReferenceException("JSON has no fps field.");
            animationData.Fps = fpsNode;
        }

        void LoadGender(JSONNode genderNode) {
            
            if (genderNode.IsNull) throw new NullReferenceException("File does not contain a gender field.");
            
            if (genderNode == animationData.Model.JsonKeys.Male) {
                animationData.Gender = Gender.Male;
            }
            else if (genderNode == animationData.Model.JsonKeys.Female) {
                animationData.Gender = Gender.Female;
            }
            else {
                throw new Exception("Unexpected value for gender in JSON file.");
            }
            
        }
    }
}
