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

        public const string LeftIndexBase = "lindex0";
        public const string LeftIndexMid = "lindex1";
        public const string LeftIndexEnd = "lindex2";
        
        public const string LeftMiddleBase = "lmiddle0";
        public const string LeftMiddleMid = "lmiddle1";
        public const string LeftMiddleEnd = "lmiddle2";
        
        public const string LeftPinkyBase = "lpinky0";
        public const string LeftPinkyMid = "lpinky1";
        public const string LeftPinkyEnd = "lpinky2";
        
        public const string LeftRingBase = "lring0";
        public const string LeftRingMid = "lring1";
        public const string LeftRingEnd = "lring2";

        public const string LeftThumbBase = "lthumb0";
        public const string LeftThumbMid = "lthumb1";
        public const string LeftThumbEnd = "lthumb2";
        
        
        public const string RightIndexBase = "rindex0";
        public const string RightIndexMid = "rindex1";
        public const string RightIndexEnd = "rindex2";
        
        public const string RightMiddleBase = "rmiddle0";
        public const string RightMiddleMid = "rmiddle1";
        public const string RightMiddleEnd = "rmiddle2";
        
        public const string RightPinkyBase = "rpinky0";
        public const string RightPinkyMid = "rpinky1";
        public const string RightPinkyEnd = "rpinky2";
        
        public const string RightRingBase = "rring0";
        public const string RightRingMid = "rring1";
        public const string RightRingEnd = "rring2";

        public const string RightThumbBase = "rthumb0";
        public const string RightThumbMid = "rthumb1";
        public const string RightThumbEnd = "rthumb2";
        
        public static SideOfBody GetSideOfBody(string name) {
            if (name.Contains(LeftPrefix) || name.Substring(0,1) == "l") return SideOfBody.Left;
            if (name.Contains(RightPrefix) || name.Substring(0,1) == "r") return SideOfBody.Right;
        
            return SideOfBody.Center;
        }

        public static readonly Dictionary<string, int> NameToJointIndex = new Dictionary<string, int> {
                                                                   {Pelvis, 0},
                                                                   {RightHip, 2},
                                                                   {LeftHip, 1},
                                                                   {Spine1, 3},
                                                                   {RightKnee, 5},
                                                                   {LeftKnee, 4},
                                                                   {Spine2, 6},
                                                                   {RightAnkle, 8},
                                                                   {LeftAnkle, 7},
                                                                   {Spine3, 9},
                                                                   {RightFoot, 11},
                                                                   {LeftFoot, 10},
                                                                   {Neck, 12},
                                                                   {RightCollar, 14},
                                                                   {LeftCollar, 13},
                                                                   {Head, 15},
                                                                   {RightShoulder, 17},
                                                                   {LeftShoulder, 16},
                                                                   {RightElbow, 19},
                                                                   {LeftElbow, 18},
                                                                   {RightWrist, 21},
                                                                   {LeftWrist, 20},
                                                                   {RightHand, 23},
                                                                   {LeftHand, 22},
                                                                   
                                                                   {LeftIndexBase, 22},
                                                                   {LeftIndexMid, 23},
                                                                   {LeftIndexEnd, 24},
        
                                                                   {LeftMiddleBase, 25},
                                                                   {LeftMiddleMid, 26},
                                                                   {LeftMiddleEnd, 27},
        
                                                                   {LeftPinkyBase, 28},
                                                                   {LeftPinkyMid, 29},
                                                                   {LeftPinkyEnd, 30},
        
                                                                   {LeftRingBase, 31},
                                                                   {LeftRingMid, 32},
                                                                   {LeftRingEnd, 33},

                                                                   {LeftThumbBase, 34},
                                                                   {LeftThumbMid, 35},
                                                                   {LeftThumbEnd, 36},
        
        
                                                                   {RightIndexBase, 37},
                                                                   {RightIndexMid, 38},
                                                                   {RightIndexEnd, 39},
        
                                                                   {RightMiddleBase, 40},
                                                                   {RightMiddleMid, 41},
                                                                   {RightMiddleEnd, 42},
        
                                                                   {RightPinkyBase, 43},
                                                                   {RightPinkyMid, 44},
                                                                   {RightPinkyEnd, 45},
        
                                                                   {RightRingBase, 46},
                                                                   {RightRingMid, 47},
                                                                   {RightRingEnd, 48},

                                                                   {RightThumbBase, 49},
                                                                   {RightThumbMid, 50},
                                                                   {RightThumbEnd, 51},
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
                    
                case LeftIndexBase:
                case LeftIndexMid:
                case LeftIndexEnd:

                case LeftMiddleBase:
                case LeftMiddleMid:
                case LeftMiddleEnd:

                case LeftPinkyBase:
                case LeftPinkyMid:
                case LeftPinkyEnd:

                case LeftRingBase:
                case LeftRingMid:
                case LeftRingEnd:

                case LeftThumbBase:
                case LeftThumbMid:
                case LeftThumbEnd:


                case RightIndexBase:
                case RightIndexMid:
                case RightIndexEnd:

                case RightMiddleBase:
                case RightMiddleMid:
                case RightMiddleEnd:

                case RightPinkyBase:
                case RightPinkyMid:
                case RightPinkyEnd:

                case RightRingBase:
                case RightRingMid:
                case RightRingEnd:

                case RightThumbBase:
                case RightThumbMid:
                case RightThumbEnd:

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