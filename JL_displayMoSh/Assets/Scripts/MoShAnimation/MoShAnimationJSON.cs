﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;


/// <summary>
/// Subclass of MoShAnimation for specific data format (JSON).
/// This class can serve as a guide for extending BMLMoShAnimation to other 
/// data formats in future. 
/// </summary>
public class MoShAnimationJSON : MoShAnimation {

    const string GenderKey = "gender";
    const string MaleString   = "male";
    const string FemaleString = "female";
    const string FPSKey = "fps";
    const string TransKey = "trans";
    const string BetasKey = "betas";
    const string PosesKey = "poses";
    

	public MoShAnimationJSON(TextAsset jsonFile)
    {
        betas = new float[10];
        LoadAnimationJSON(jsonFile);
    }


    /// <summary>
    /// Load a new animation from a JSON TextAsset, replacing the values of the previous animation.
    /// </summary>
    /// <param name="jsonFile"></param>
    /// <exception cref="Exception"></exception>
    void LoadAnimationJSON (TextAsset jsonFile) 
	{
        if (jsonFile == null) throw new NullReferenceException("Tried to instantiate Animation JSON with TextAsset == null");
        
		JSONNode jsonNode = JSON.Parse (jsonFile.text);
		LoadAnimationJSON (jsonNode);
	}


    void LoadAnimationJSON(JSONNode moshJSON)
	{
		// set gender. 
        JSONNode genderNode = moshJSON[GenderKey];
        if (genderNode.IsNull) throw new NullReferenceException("File does not contain a gender field.");
        
        if (genderNode == MaleString) {
			gender = Genders.MALE;
            Jc = JointCalculator.Male;
		} else {
            if (genderNode == FemaleString) {
                gender = Genders.FEMALE;
                Jc = JointCalculator.Female;
            } else {
                throw new Exception ("Unexpected value for gender in JSON file.");
            }
        }
        
        JSONNode fpsNode = moshJSON[FPSKey];
        if (fpsNode.IsNull) throw new NullReferenceException("JSON has no fps field.");
        fps = fpsNode;
        SourceFPS = fps;

        
        length = moshJSON[TransKey].Count;
        SourceLength = length;
        duration = length / (float)fps;

        for (int i = 0; i < 10; i++) {
            
            betas[i] = moshJSON[BetasKey][i];
        }
                
        Translation = new Vector3[length];
        Poses = new Quaternion[length, JointCount];
        for (int i = 0; i < length; i++) {
            
            // original code has x flipped, because Unity has it's z axis flipped
            // compared to other software. I don't know why this would require 
            // flipping the x axis. This might be an error.
            // Oh... this might be because the object was rotated earlier. 
            // possibly worth investigating.
            // I feel like some of the flips and rotations might be redundant, but 
            // it's a bit risky breaking them!

            // I'm pretty sure maya is right handed z-up. 
            // Unity is right handed y up? 
            JSONNode transNode = moshJSON[TransKey];
            float x = transNode[i][0];
            float y = transNode[i][1];
            float z = transNode[i][2];
            if (ZAxisUp) {
                x = -x;
            }
            else {
                y = -y;
            }
            Translation[i] = new Vector3(x, y, z);

            // read the quaternions in. 
            for (int j = 0; j < JointCount; j++) {
                // Quaternion components must also be flipped. But the original didn't check what the up axis is. 
                // Arrrggg the error was that it was getting cast to an integer or something because I was multiplying by -1, not -1f.
                JSONNode posesNode = moshJSON[PosesKey];
                float qx = -1.0f * posesNode[i][j][0];
                float qy = posesNode[i][j][1];
                float qz = posesNode[i][j][2];
                float qw = -1.0f * posesNode[i][j][3];
                //Debug.Log("frame " + i + ": qx = " + qx + " qy = " + qy + "qz = " + qz + " qw = " + qw);
                Poses[i, j] = new Quaternion(qx, qy, qz, qw);

            }
        }
    }


}
