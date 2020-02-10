using System.Collections.Generic;
using UnityEngine;
// ReSharper disable MemberCanBePrivate.Global

namespace MoshPlayer.Scripts.BML.SMPLModel {
    public static class Bones {
        public const string LeftPrefix1  = "L_";
        public const string RightPrefix1 = "R_";
        
        public const string LeftPrefix2  = "l";
        public const string RightPrefix2 = "r";
            
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
                                                                                                          {RightHand, 23},
                                                                                                          
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

        public static SideOfBody GetSideOfBody(string name) {
            if (name.Contains(LeftPrefix1) || name.Substring(0,1) == LeftPrefix2) return SideOfBody.Left;
            if (name.Contains(RightPrefix1) || name.Substring(0,1) == RightPrefix2) return SideOfBody.Right;
        
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
        
        /// <summary>
        /// Position of bones must be adjusted carefully, since moving a parent after setting the child's
        /// position will then move the child away from desired location. This function traverses through
        /// a hierarchy to ensure this never happens.
        /// </summary>
        /// <param name="parentBone"></param>
        /// <param name="rootCoordinateTransform"></param>
        /// <param name="jointPositions"></param>
        public static void SetPositionDownwardsThroughHierarchy(Transform parentBone, Transform rootCoordinateTransform, Vector3[] jointPositions) {
            string boneName = parentBone.name;
            if (NameToJointIndex.TryGetValue(boneName, out int boneJointIndex)) {
                parentBone.position = rootCoordinateTransform.TransformPoint(jointPositions[boneJointIndex]);
                foreach (Transform child in parentBone) {
                    SetPositionDownwardsThroughHierarchy(child, rootCoordinateTransform, jointPositions);
                }
            }
        }
        
        public static void ResetBonesDownwardsThroughHierarchy(Transform parentBone, Transform rootCoordinateTransform, Vector3[] originalPositions) {
            string boneName = parentBone.name;
            if (NameToJointIndex.TryGetValue(boneName, out int boneJointIndex)) {
                parentBone.position = rootCoordinateTransform.TransformPoint(originalPositions[boneJointIndex]);
                parentBone.rotation = Quaternion.identity;
                foreach (Transform child in parentBone) {
                    SetPositionDownwardsThroughHierarchy(child, rootCoordinateTransform, originalPositions);
                }
            }
        }
        
    }

    public enum SideOfBody {
        Left,
        Right,
        Center
    }
    
    
    
}