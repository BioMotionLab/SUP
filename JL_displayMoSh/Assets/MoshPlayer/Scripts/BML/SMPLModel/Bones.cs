using System.Collections.Generic;
using UnityEngine;
// ReSharper disable MemberCanBePrivate.Global

namespace MoshPlayer.Scripts.BML.SMPLModel {
    public static class Bones {
        public const string LeftPrefix  = "L_";
        public const string RightPrefix = "R_";
            
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

        public static readonly Dictionary<string, int> NameToJointIndex = new Dictionary<string, int> {
                                                                   {Pelvis, 0},
                                                                   {LeftHip, 1},
                                                                   {RightHip, 2},
                                                                   {Spine1, 3},
                                                                   {LeftKnee, 4},
                                                                   {RightKnee, 5},
                                                                   {Spine2, 6},
                                                                   {LeftAnkle, 7},
                                                                   {RightAnkle, 8},
                                                                   {Spine3, 9},
                                                                   {LeftFoot, 10},
                                                                   {RightFoot, 11},
                                                                   {Neck, 12},
                                                                   {LeftCollar, 13},
                                                                   {RightCollar, 14},
                                                                   {Head, 15},
                                                                   {LeftShoulder, 16},
                                                                   {RightShoulder, 17},
                                                                   {LeftElbow, 18},
                                                                   {RightElbow, 19},
                                                                   {LeftWrist, 20},
                                                                   {RightWrist, 21},
                                                                   {LeftHand, 22},
                                                                   {RightHand, 23}
                                                               };
        

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

        public static bool IsFootBone(Transform bone) {
            string boneName = bone.name;
            return boneName == LeftFoot || boneName == RightFoot;
        }
    }

    public enum SideOfBody {
        Left,
        Right,
        Center
    }
}