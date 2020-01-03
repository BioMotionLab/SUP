using System;
using System.Collections;
using System.Linq;
using System.Text;
using MoshPlayer.Scripts.BML.SMPLModel;
using MoshPlayer.Scripts.ThirdParty.SimpleJSON;
using MoshPlayer.Scripts.Utilities;
using UnityEditor;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.FileLoaders {
    /// <summary>
    /// Subclass of MoshAnimation for specific data format (JSON).
    /// This class can serve as a guide for extending BMLMoShAnimation to other 
    /// data formats in future. 
    /// </summary>
    public class MoshAnimationFromJSON {

        Gender        gender;
        float[]       betas;
        int           fps;
        int           frameCount;
        Vector3[]     translations;
        Quaternion[,] poses;
        ModelDefinition matchedModel;
        readonly SMPLSettings settings;

        public MoshAnimationFromJSON(string jsonFileWholeString, SMPLSettings settings)  {
            if (jsonFileWholeString == null) throw new NullReferenceException("Tried to instantiate Animation JSON with null TextAsset");
            if (settings == null) throw new NullReferenceException("No Settings specified");
            this.settings = settings;
            // AB: This is where the load speed bottleneck is. It seems to have trouble parsing even fairly small ~1MB files (~100ms or more).
            // Once parsed it seems to be very fast to load (< 5 ms)
            // According to Google, SimpleJSON is the fastest parser... so perhaps just a limitation of the way the data is stored. 
            JSONNode jsonNode = JSON.Parse(jsonFileWholeString); 
            
            LoadAnimationFromJSON (jsonNode);
        }

        public MoshAnimation BuildWithSettings() {
            MoshAnimation animation = new MoshAnimation(matchedModel, gender, frameCount, fps, betas, translations, poses);
            return animation;
        }
    
        void LoadAnimationFromJSON(JSONNode moshJSON) {
            
            JSONNode genderNode = null;
            JSONNode betasNode = null;
            JSONNode fpsNode = null;
            JSONNode transNode = null;
            JSONNode posesNode = null;

            foreach (var model in settings.ModelParameters) {
                genderNode = moshJSON[model.JsonKeys.Gender];
                betasNode = moshJSON[model.JsonKeys.Betas];
                fpsNode = moshJSON[model.JsonKeys.FPS];
                transNode = moshJSON[model.JsonKeys.Translations];
                posesNode = moshJSON[model.JsonKeys.Poses];

                if (ModelMatch(betasNode, model)) {
                    matchedModel = model;
                    //Debug.Log($"Matched model {matchedModel.ModelName}");
                    break;
                }
            }

            if (matchedModel == null) {
                Debug.LogError("Could not match animation to a model");
                return;
            }
            
            
            LoadGender(genderNode);
            Debug.Log($"Gender: {gender}");
            LoadFPS(fpsNode);
            Debug.Log($"fps: {fps}");
            LoadBetas(betasNode);
            if (fps == 0) {
                Debug.Log($"No fps specified, defaulting to {settings.FallbackFPS}");
                fps = settings.FallbackFPS;
            }
            
            DebugArray("betas", betas.ToList());
            
            LoadTranslationsAndPosesFromJoints(transNode, posesNode);
            DebugArray("trans" , translations.ToList());
            //DebugArray("poses" , poses.ToList());
            
        }

        void DebugArray(string name, IList list)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{name}: length {list.Count}");
            foreach (var item in list) sb.AppendLine(item.ToString());
            Debug.Log(sb);
        }
        

        static bool ModelMatch(JSONNode betasNode, ModelDefinition model) {
            bool modelMatch = betasNode.Count == model.BodyShapeBetaCount;
            return modelMatch;
        }
        

        void LoadTranslationsAndPosesFromJoints(JSONNode transNode, JSONNode posesNode) {
            
            frameCount = transNode.Count;
            
            translations = new Vector3[frameCount];
            poses = new Quaternion[frameCount, matchedModel.JointCount];
            
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
                LoadTranslationFromJoint(transNode, frameIndex);
                for (int jointIndex = 0; jointIndex < matchedModel.JointCount; jointIndex++) {
                    LoadPosesFromJoint(posesNode, frameIndex, jointIndex);
                }
            }
        }

        void LoadTranslationFromJoint(JSONNode transNode, int frameIndex) {
            JSONNode thisTranslation = transNode[frameIndex];
            Vector3 translationInMayaCoords = new Vector3(thisTranslation[0], thisTranslation[1], thisTranslation[2]);
            Vector3 translationInUnityCoords = translationInMayaCoords.ToLeftHanded();
            translations[frameIndex] = translationInUnityCoords;
        }

        void LoadPosesFromJoint(JSONNode posesNode, int frameIndex, int jointIndex) {
            JSONNode thisPoseJson = posesNode[frameIndex][jointIndex];
            Quaternion poseInMayaCoords = new Quaternion(thisPoseJson[0], thisPoseJson[1], thisPoseJson[2], thisPoseJson[3]);
            Quaternion poseInUnityCoords = poseInMayaCoords.ToLeftHanded();
            poses[frameIndex, jointIndex] = poseInUnityCoords;
        }


        void LoadBetas(JSONNode betasNode) {
            betas = new float[matchedModel.BodyShapeBetaCount];
            for (int i = 0; i < matchedModel.BodyShapeBetaCount; i++) {
                betas[i] = betasNode[i];
            }
        }

        void LoadFPS(JSONNode fpsNode) {
            
            if (fpsNode.IsNull) throw new NullReferenceException("JSON has no fps field.");
            fps = fpsNode;
        }

        void LoadGender(JSONNode genderNode) {
            
            if (genderNode.IsNull) throw new NullReferenceException("File does not contain a gender field.");
            
            if (genderNode == matchedModel.JsonKeys.Male) {
                gender = Gender.Male;
            }
            else if (genderNode == matchedModel.JsonKeys.Female) {
                gender = Gender.Female;
            }
            else {
                throw new Exception("Unexpected value for gender in JSON file.");
            }
            
        }
    }
}
