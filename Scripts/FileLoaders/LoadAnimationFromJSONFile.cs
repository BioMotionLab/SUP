using System;
using System.IO;
using System.Threading.Tasks;
using SMPLModel;
using ThirdParty.SimpleJSON;
using UnityEngine;
using Utilities;

namespace FileLoaders {

    public class AnimationJsonParser {
        JSONNode jsonNode;
        AnimationData animationData;
        readonly string jsonString;

        public AnimationJsonParser(string jsonString) {
            this.jsonString = jsonString;
        }

        public async Task<AnimationData> LoadDataAsync() {
            animationData = new AnimationData();
            jsonNode = await Task.Run(() => JSON.Parse(jsonString) );
            return animationData;
        }
        
        public void FormatData() {
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

            //DebugArray("betas", betas.ToList());
            
            LoadTranslationsAndPosesFromJoints(transNode, posesNode);
            //DebugArray("trans" , translations.ToList());
            //DebugArray("poses" , poses.ToList());
        }

        public bool IsMatchingModel(ModelDefinition model) {
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
    
    public class LoadAnimationFromJsonTextAsset : AnimationLoadStrategy {
        
        readonly TextAsset textAsset;
        AnimationJsonParser jsonParser;

        public LoadAnimationFromJsonTextAsset(TextAsset textAsset, Models models) : base (models) {
            this.textAsset = textAsset;
            jsonParser = new AnimationJsonParser(textAsset.text);
        }
        
        protected override async Task<AnimationData> LoadDataWithStrategy() {
            AnimationData animationData = await jsonParser.LoadDataAsync();
            return animationData;
        }

        protected override bool IsMatchingModel(ModelDefinition model) {
            return jsonParser.IsMatchingModel(model);
        }

        protected override void FormatData() {
            jsonParser.FormatData();
        }
    }
    
    
    public class LoadAnimationFromJSONFile : AnimationLoadFromFile {
        
        AnimationJsonParser jsonParser;

        public LoadAnimationFromJSONFile(string filePath, Models possibleModels): 
            base(filePath, possibleModels) { }
        

        protected override async Task<AnimationData> LoadDataWithStrategy() {
            // AB: This is where the load speed bottleneck is. It seems to have trouble parsing even fairly small ~1MB files (~100ms or more).
            // Once parsed it seems to be very fast to load (< 5 ms)
            // According to Google, SimpleJSON is the fastest parser... so perhaps just a limitation of the way the data is stored. 
            
            string jsonString = File.ReadAllText(filePath);
            jsonParser = new AnimationJsonParser(jsonString);
            AnimationData animationData = await jsonParser.LoadDataAsync();
            return animationData;
        }

        protected override bool IsMatchingModel(ModelDefinition model) {
            return jsonParser.IsMatchingModel(model);
        }

        protected override void FormatData() {
            jsonParser.FormatData();
        }
    }
}
