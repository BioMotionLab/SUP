using System;
using System.CodeDom;
using System.Text.RegularExpressions;
using MoshPlayer.Scripts.Playback;
using MoshPlayer.Scripts.SMPLModel;
using MoshPlayer.Scripts.Utilities;
using UnityEngine;



namespace MoshPlayer.Scripts.FileLoaders {
    public class LoadFromH5 : MonoBehaviour {

        const int MaxStringLengthForReading = 150;
        
        [SerializeField] ModelDefinition model = default;
        [SerializeField] PlaybackOptions playbackOptions = default;
        [SerializeField] string h5FilePath = "test_data.h5";

        
        Gender gender;
        float[] betas;
        int fps;
        int frameCount;
        Vector3[] translations;
        Quaternion[,] poses;

        string filePath;
        [SerializeField]
        bool showDebug = true;
        
        void Start() {
            filePath = Application.dataPath + "/" + h5FilePath;
            betas = ReadBetas();
            gender = ReadGender();
            fps = ReadFPS();
            if (fps == 0) {
                Debug.LogWarning($"No fps specified, defaulting to {playbackOptions.FallbackFPS} fps");
                fps = playbackOptions.FallbackFPS;
            }

            LoadTranslationAndPosesFromJoints();
            
            
            if (showDebug) {
                Debug.Log($"betas length = {betas.Length}");
                foreach (float beta in betas) {
                    Debug.Log($"\tbeta: {beta}");
                }
                Debug.Log($"gender: {gender}");
                Debug.Log($"fps: {fps}", this);
                
            }

        }

        void LoadTranslationAndPosesFromJoints() {
            OpenedH5DataSet translationDataset = new OpenedH5DataSet(filePath, model.H5Keys.Translations);
            OpenedH5DataSet posesDataset = new OpenedH5DataSet(filePath, model.H5Keys.Poses);
            
            frameCount = translationDataset.Dimensions[0];

            foreach (int dim in posesDataset.Dimensions) {
                Debug.Log($"dim: {dim}");
            }
            
            
            double[] translationVectorizedDoubles = translationDataset.LoadAs<double>(frameCount*3);
            double[] posesVectorizedDoubles = posesDataset.LoadAs<double>(frameCount * model.JointCount * 3);
            
            float[] translationVectorized = ToFloatArray(translationVectorizedDoubles);
            float[] posesVectorized = ToFloatArray(posesVectorizedDoubles);
            
            
            translations = new Vector3[frameCount];
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
                GetTranslationAtFrame(frameIndex, translationVectorized);
                for (int jointIndex = 0; jointIndex < model.JointCount; jointIndex++) {
                    GetTranslationAtFrame(frameIndex, jointIndex, posesVectorized);
                }
            }
           

         
        }

        void GetTranslationAtFrame(int frameIndex, int jointIndex, float[] posesVectorized) {
            int vectorIndex = frameIndex * model.JointCount * 3 + jointIndex * 3;
            var rx = posesVectorized[vectorIndex];
            var ry = posesVectorized[vectorIndex + 1];
            var rz = posesVectorized[vectorIndex + 2];
            Vector3 rotationVectorInMayaCoords = new Vector3(rx, ry, rz);
            Quaternion quaternionInMayaCoords = RotationVectorToQuaternion(rotationVectorInMayaCoords);
            //Debug.Log($"Frame: {frameIndex}, Joint: {jointIndex}: rotvec: {rotationVectorInMayaCoords.ToString("F4")} quat: {quaternionInMayaCoords.ToString("F4")}");
            Quaternion quaternionInUnityCoords = quaternionInMayaCoords.ToLeftHanded();
            poses[frameIndex, jointIndex] = quaternionInUnityCoords;
        }

        /// <summary>
        /// Adapted from https://github.com/facebookresearch/QuaterNet/blob/master/common/quaternion.py [expmap_to_quaternion()]
        /// With help from Saeed Ghorbani
        /// </summary>
        /// <param name="rotationVector"></param>
        /// <returns></returns>
        Quaternion RotationVectorToQuaternion(Vector3 rotationVector) {

            float theta = rotationVector.magnitude;
            
            var qx = 0.5f * SinC(0.5f * (theta/Mathf.PI))*rotationVector.x;
            var qy = 0.5f * SinC(0.5f * (theta/Mathf.PI))*rotationVector.y;
            var qz = 0.5f * SinC(0.5f * (theta/Mathf.PI))*rotationVector.z;
            var qw = Mathf.Cos(0.5f * theta);
            Quaternion quat = new Quaternion(qx, qy, qz, qw);
            return quat;
        }

        float SinC(float x) {
            float result = Mathf.Sin(Mathf.PI * x) / (Mathf.PI * x);
            return result;
        }

        void GetTranslationAtFrame(int frameIndex, float[] translationVectorizedFloats) {
            int vectorIndex = frameIndex * 3;
            var x = translationVectorizedFloats[vectorIndex];
            var y = translationVectorizedFloats[vectorIndex + 1];
            var z = translationVectorizedFloats[vectorIndex + 2];
            Vector3 translationInMayaCoords = new Vector3(x, y, z);
            Vector3 translationInUnityCoords = translationInMayaCoords.ConvertTranslationFromMayaToUnity();
            translations[frameIndex] = translationInUnityCoords;
        }

        int ReadFPS() {
            OpenedH5DataSet fpsDataset = new OpenedH5DataSet(filePath, model.H5Keys.FPS);

            double[] fpsArray = fpsDataset.LoadAs<double>(1);

            int loadedFPS = (int)Math.Round(fpsArray[0]);
            return loadedFPS;
        }

        float[] ReadBetas() {
            OpenedH5DataSet betasDataset = new OpenedH5DataSet(filePath, model.H5Keys.Betas);
        
            double[] betaArray = betasDataset.LoadAs<double>(model.BodyShapeBetaCount);
        
            float[] finalBetas = ToFloatArray(betaArray);
            return finalBetas;
        }

        static float[] ToFloatArray(double[] betaArray) {
            float[] floatArr = new float[betaArray.Length];
            for (int index = 0; index < betaArray.Length; index++) {
                floatArr[index] = (float) betaArray[index];
            }

            return floatArr;
        }

        Gender ReadGender() {
            OpenedH5DataSet genderDataSet = new OpenedH5DataSet(filePath, model.H5Keys.Gender);
        
            byte[] loadedBytes = genderDataSet.LoadAs<byte>(MaxStringLengthForReading);
        
            string readString = System.Text.Encoding.ASCII.GetString(loadedBytes);
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            string replacedString = rgx.Replace(readString, "");
            
            if (replacedString == model.H5Keys.Male) return Gender.Male;
            else if (replacedString == model.H5Keys.Female) return Gender.Female;
            else throw new Exception("Unexpected value for gender in JSON file.");
        }
    
    }
}