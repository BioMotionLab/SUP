using System;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    public class Bones {
        const string LeftPrefix  = "L_";
        const string RightPrefix = "R_";
            
        public const string Pelvis = "Pelvis";
        public const string LeftHip    = "L_Hip";
        public const string RightHip   = "R_Hip";
        public const string Spine1     = "Spine1";
        public const string Spine2     = "Spine2";
        public const string Spine3     = "Spine3";
        
        public const string LeftKnee  = "L_Knee";
        public const string RightKnee = "R_Knee";
        
        public const string LeftAnkle  = "L_Ankle";
        public const string RightAnkle = "R_Ankle";
        
        public const string LeftFoot      = "L_Foot";
        public const string RightFoot     = "R_Foot";
        public const string Neck          = "Neck";
        public const string LeftCollar    = "L_Collar";
        public const string RightCollar   = "R_Collar";
        public const string Head          = "Head";
        public const string LeftShoulder  = "L_Shoulder";
        public const string RightShoulder = "R_Shoulder";
        public const string LeftElbow     = "L_Elbow";
        public const string RightElbow    = "R_Elbow";
        public const string LeftWrist     = "L_Wrist";
        public const string RightWrist    = "R_Wrist";
        public const string LeftHand      = "L_Hand";
        public const string RightHand     = "R_Hand";

        public static SideOfBody GetSideOfBody(string name) {
            if (name.Contains(LeftPrefix)) return SideOfBody.Left;
            if (name.Contains(RightPrefix)) return SideOfBody.Right;
            return SideOfBody.Center;
        }

        public static bool IsBone(Transform child) {
            switch (child.name) {
                case Pelvis:
                case LeftHip:
                case RightHip:
                case Spine1:
                case Spine2:
                case Spine3:
                case LeftKnee:
                case RightKnee:
                case LeftAnkle:
                case RightAnkle :
                case LeftFoot:
                case RightFoot: 
                case Neck: 
                case LeftCollar: 
                case RightCollar: 
                case Head: 
                case LeftShoulder: 
                case RightShoulder: 
                case LeftElbow:
                case RightElbow:
                case LeftWrist:
                case RightWrist:
                case LeftHand:
                case RightHand:
                    return true;
                default:
                    return false;
            }
        }
    }

    public enum SideOfBody {
        Left,
        Right,
        Center
    }
}