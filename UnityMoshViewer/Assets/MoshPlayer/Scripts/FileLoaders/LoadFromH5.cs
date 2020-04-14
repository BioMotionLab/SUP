using System;
using System.Text.RegularExpressions;
using MoshPlayer.Scripts.SMPLModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;


namespace MoshPlayer.Scripts.FileLoaders {
    public class LoadFromH5 : MonoBehaviour {

        const int MaxStringLengthForReading = 150;
        
        [FormerlySerializedAs("matchedModel")] [SerializeField] ModelDefinition model;
        [SerializeField] string h5FilePath = "test_data.h5";

        
        
        Gender gender;
        float[] betas;
        int fps;
        
        
        string filePath;
        bool showDebug = true;
        
        void Start() {
            filePath = Application.dataPath + "/" + h5FilePath;
            betas = ReadBetas();
            gender = ReadGender();
            fps = ReadFPS();
            
            
            if (showDebug) {
                Debug.Log($"betas length = {betas.Length}");
                foreach (float beta in betas) {
                    Debug.Log($"\tbeta: {beta}");
                }
                Debug.Log($"gender: {gender}");
                Debug.Log($"fps: {fps}", this);
                
            }
            
            
            
        }

        int ReadFPS() {
            OpenedH5DataSet fpsDataset = new OpenedH5DataSet(filePath, model.JsonKeys.FPS);

            int[] fpsArray = fpsDataset.LoadAs<int>(1);

            int fps = fpsArray[0];
            return fps;
        }

        float[] ReadBetas() {
            OpenedH5DataSet betasDataset = new OpenedH5DataSet(filePath, model.JsonKeys.Betas);
        
            double[] betaArray = betasDataset.LoadAs<double>(model.BodyShapeBetaCount);
        
            float[] floatArr = new float[betaArray.Length];
            for (int index = 0; index < betaArray.Length; index++) {
                floatArr[index] = (float) betaArray[index];
            }
            return floatArr;
        }
    
        Gender ReadGender() {
            OpenedH5DataSet genderDataSet = new OpenedH5DataSet(filePath, model.JsonKeys.Gender);
        
            byte[] loadedBytes = genderDataSet.LoadAs<byte>(MaxStringLengthForReading);
        
            string readString = System.Text.Encoding.ASCII.GetString(loadedBytes);
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            string replacedString = rgx.Replace(readString, "");
            
            if (replacedString == model.JsonKeys.Male) return Gender.Male;
            else if (replacedString == model.JsonKeys.Female) return Gender.Female;
            else throw new Exception("Unexpected value for gender in JSON file.");
        }
    
    }
}