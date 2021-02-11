using System.Security.Permissions;
using JetBrains.Annotations;
using MoshPlayer.Scripts.SMPLModel;
using UnityEngine;

namespace MoshPlayer.Scripts.FileLoaders {
    public class AnimationData {
        public ModelDefinition Model;
        public Gender        Gender;
        public float[]       Betas;
        public int           Fps;
        public int           FrameCount;
        public Vector3[]     Translations;
        public Quaternion[,] Poses;
        
        [PublicAPI]
        public void ShowDebug() {
            Debug.Log($"betas length = {Betas.Length}");
            foreach (float beta in Betas) {
                Debug.Log($"\tbeta: {beta}");
            }
            Debug.Log($"gender: {Gender}");
            Debug.Log($"fps: {Fps}");
            Debug.Log($"FrameCount: {FrameCount}");
            Debug.Log($"Translations count: {Translations.Length}, " +
                      $"translations[0]: {Translations[0].ToString("F2")}");
            Debug.Log($"poses count: ({Poses.GetLength(0)},{Poses.GetLength(1)}), " +
                      $"Poses[0,0]: {Poses[0,0].ToString("F2")}");
                
        }
        
    }
}