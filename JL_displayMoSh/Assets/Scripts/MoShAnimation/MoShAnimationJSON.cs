﻿
using UnityEngine;
using SimpleJSON;
using System;


/// <summary>
/// Subclass of MoShAnimation for specific data format (JSON).
/// This class can serve as a guide for extending BMLMoShAnimation to other 
/// data formats in future. 
/// </summary>
public class MoShAnimationJSON {

    const string GenderKey = "gender";
    const string MaleString   = "male";
    const string FemaleString = "female";
    const string FPSKey = "fps";
    const string TransKey = "trans";
    const string BetasKey = "betas";
    const string PosesKey = "poses";

    Gender gender;
    float[] betas;
    int sourceFPS;
    int sourceTotalFrameCount;
    Vector3[] translation;
    Quaternion[,] poses;

    public MoShAnimationJSON(TextAsset jsonFile)  {
        if (jsonFile == null) throw new NullReferenceException("Tried to instantiate Animation JSON with null TextAsset");

        JSONNode jsonNode = JSON.Parse (jsonFile.text);
        LoadAnimationJSON (jsonNode);
    }

    public MoShAnimation Build() {
        MoShAnimation animation = new MoShAnimation(gender, sourceTotalFrameCount, sourceFPS, betas, translation, poses);
        return animation;
    }
    
    void LoadAnimationJSON(JSONNode moshJSON)
	{
		LoadGender(moshJSON);
        LoadFPS(moshJSON);
        
        JSONNode transNode = moshJSON[TransKey];
        sourceTotalFrameCount = transNode.Count;

        LoadBetas(moshJSON);
        LoadTranslationAndPoses(moshJSON, transNode, sourceTotalFrameCount);
    }

    void LoadTranslationAndPoses(JSONNode moshJSON, JSONNode transNode, int totalNumberOfFrames) {
        translation = new Vector3[totalNumberOfFrames];
        poses = new Quaternion[totalNumberOfFrames, MoShAnimation.JointCount];
        for (int frameIndex = 0; frameIndex < totalNumberOfFrames; frameIndex++) {
            // original code has x flipped, because Unity has it's z axis flipped
            // compared to other software. I don't know why this would require 
            // flipping the x axis. This might be an error.
            // Oh... this might be because the object was rotated earlier. 
            // possibly worth investigating.
            // I feel like some of the flips and rotations might be redundant, but 
            // it's a bit risky breaking them!

            // I'm pretty sure maya is right handed z-up. 
            // Unity is right handed y up? 

            JSONNode thisTranslation = transNode[frameIndex];
            float x = thisTranslation[0];
            float y = thisTranslation[1];
            float z = thisTranslation[2];
            if (MoShAnimation.ZAxisUp) {
                x = -x;
            }
            else {
                y = -y;
            }

            Vector3 flippedTranslation = new Vector3(x, y, z);
            translation[frameIndex] = flippedTranslation;

            // read the quaternions in. 
            for (int jointIndex = 0; jointIndex < MoShAnimation.JointCount; jointIndex++) {
                // Quaternion components must also be flipped. But the original didn't check what the up axis is. 
                // Arrrggg the error was that it was getting cast to an integer or something because I was multiplying by -1, not -1f.
                JSONNode posesNode = moshJSON[PosesKey];
                JSONNode thisPose = posesNode[frameIndex][jointIndex];
                float qx = -1.0f * thisPose[0];
                float qy = thisPose[1];
                float qz = thisPose[2];
                float qw = -1.0f * thisPose[3];
                poses[frameIndex, jointIndex] = new Quaternion(qx, qy, qz, qw);
            }
        }
    }

    void LoadBetas(JSONNode moshJSON) {
        betas = new float[10];
        for (int i = 0; i < 10; i++) {
            betas[i] = moshJSON[BetasKey][i];
        }
    }

    void LoadFPS(JSONNode moshJSON) {
        JSONNode fpsNode = moshJSON[FPSKey];
        if (fpsNode.IsNull) throw new NullReferenceException("JSON has no fps field.");
        sourceFPS = fpsNode;
    }

    void LoadGender(JSONNode moshJSON) {
        JSONNode genderNode = moshJSON[GenderKey];
        if (genderNode.IsNull) throw new NullReferenceException("File does not contain a gender field.");

        if (genderNode == MaleString) {
            gender = Gender.MALE;
        }
        else {
            if (genderNode == FemaleString) {
                gender = Gender.FEMALE;
            }
            else {
                throw new Exception("Unexpected value for gender in JSON file.");
            }
        }
    }
}
