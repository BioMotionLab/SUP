using System;
using MoshPlayer.Scripts.BML.SMPLModel;
using MoshPlayer.Scripts.ThirdParty.SimpleJSON;
using MoshPlayer.Scripts.Utilities;
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

        public MoshAnimationFromJSON(string jsonFileWholeString)  {
            if (jsonFileWholeString == null) throw new NullReferenceException("Tried to instantiate Animation JSON with null TextAsset");

            // AB: This is where the load speed bottleneck is. It seems to have trouble parsing even fairly small ~1MB files (~100ms or more).
            // Once parsed it seems to be very fast to load (< 5 ms)
            // According to Google, SimpleJSON is the fastest parser... so perhaps just a limitation of the way the data is stored. 
            JSONNode jsonNode = JSON.Parse(jsonFileWholeString); 
            
            LoadAnimationFromJSON (jsonNode);
        }

        public MoshAnimation BuildWithSettings(SMPLSettings settings) {
            MoshAnimation animation = new MoshAnimation(gender, frameCount, fps, betas, translations, poses);
            return animation;
        }
    
        void LoadAnimationFromJSON(JSONNode moshJSON)
        {
            LoadGender(moshJSON);
            LoadFPS(moshJSON);
            
            LoadBetas(moshJSON);
            
            LoadTranslationsAndPosesFromJoints(moshJSON);
            

        }

        void LoadTranslationsAndPosesFromJoints(JSONNode moshJSON) {
            JSONNode     transNode = moshJSON[SMPLConstants.JSONKeys.Translations];
            frameCount = transNode.Count;
            
            translations = new Vector3[frameCount];
            poses = new Quaternion[frameCount, SMPLConstants.JointCount];
            
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
                LoadTranslationFromJoint(transNode, frameIndex);
                for (int jointIndex = 0; jointIndex < SMPLConstants.JointCount; jointIndex++) {
                    LoadPosesFromJoint(moshJSON, frameIndex, jointIndex);
                }
            }
        }

        void LoadTranslationFromJoint(JSONNode transNode, int frameIndex) {
            JSONNode thisTranslation = transNode[frameIndex];
            Vector3 translationInMayaCoords = new Vector3(thisTranslation[0], thisTranslation[1], thisTranslation[2]);
            Vector3 translationInUnityCoords = translationInMayaCoords.ToLeftHanded();
            translations[frameIndex] = translationInUnityCoords;
        }

        void LoadPosesFromJoint(JSONNode moshJSON, int frameIndex, int jointIndex) {
            JSONNode posesNode = moshJSON[SMPLConstants.JSONKeys.Poses];
            JSONNode thisPoseJson = posesNode[frameIndex][jointIndex];
            Quaternion poseInMayaCoords = new Quaternion(thisPoseJson[0], thisPoseJson[1], thisPoseJson[2], thisPoseJson[3]);
            Quaternion poseInUnityCoords = poseInMayaCoords.ToLeftHanded();
            poses[frameIndex, jointIndex] = poseInUnityCoords;
        }


        void LoadBetas(JSONNode moshJSON) {
            betas = new float[10];
            for (int i = 0; i < SMPLConstants.BodyShapeBetaCount; i++) {
                betas[i] = moshJSON[SMPLConstants.JSONKeys.Betas][i];
            }
        }

        void LoadFPS(JSONNode moshJSON) {
            JSONNode fpsNode = moshJSON[SMPLConstants.JSONKeys.FPS];
            if (fpsNode.IsNull) throw new NullReferenceException("JSON has no fps field.");
            fps = fpsNode;
        }

        void LoadGender(JSONNode moshJSON) {
            JSONNode genderNode = moshJSON[SMPLConstants.JSONKeys.Gender];
            if (genderNode.IsNull) throw new NullReferenceException("File does not contain a gender field.");
            
            if (genderNode == SMPLConstants.JSONKeys.Male) {
                gender = Gender.Male;
            }
            else if (genderNode == SMPLConstants.JSONKeys.Female) {
                gender = Gender.Female;
            }
            else {
                throw new Exception("Unexpected value for gender in JSON file.");
            }
            
        }
    }
}
