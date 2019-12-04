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

            JSONNode jsonNode = JSON.Parse(jsonFileWholeString);
            LoadAnimationJSON (jsonNode);
        }

        public MoshAnimation BuildWithSettings(SMPLSettings settings) {
            MoshAnimation animation = new MoshAnimation(gender, frameCount, fps, betas, translations, poses, settings);
            return animation;
        }
    
        void LoadAnimationJSON(JSONNode moshJSON)
        {
            LoadGender(moshJSON);
            LoadFPS(moshJSON);
        
            

            LoadBetas(moshJSON);
            LoadTranslations(moshJSON);
            LoadPoses(moshJSON);
        }

        void LoadTranslations(JSONNode moshJSON) {
            JSONNode     transNode = moshJSON [SMPLConstants.JSONKeys.Translations];
            frameCount = transNode.  Count;
            
            translations = new Vector3 [frameCount];

            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
                JSONNode thisTranslation = transNode[frameIndex];
                Vector3 translationInMayaCoords = new Vector3(thisTranslation[0], thisTranslation[1], thisTranslation[2]);
                Vector3 translationInUnityCoords = translationInMayaCoords.ToLeftHanded();
                translations[frameIndex] = translationInUnityCoords;
            }
        }

        
        void LoadPoses(JSONNode moshJSON) {
            
            poses = new Quaternion[frameCount, SMPLConstants.JointCount];
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
                for (int jointIndex = 0; jointIndex < SMPLConstants.JointCount; jointIndex++) {
                    JSONNode posesNode = moshJSON[SMPLConstants.JSONKeys.Poses];
                    JSONNode thisPoseJson = posesNode[frameIndex][jointIndex];
                    Quaternion poseInMayaCoords = new Quaternion(thisPoseJson[0], thisPoseJson[1], thisPoseJson[2], thisPoseJson[3]);
                    var quaternionInUnityCoords = poseInMayaCoords.ToLeftHanded();
                    poses[frameIndex, jointIndex] = quaternionInUnityCoords;
                }
            }
        }

        void LoadBetas(JSONNode moshJSON) {
            betas = new float[10];
            for (int i = 0; i < 10; i++) {
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
