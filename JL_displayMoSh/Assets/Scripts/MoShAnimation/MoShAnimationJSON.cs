using System.Collections;
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
    /// <summary>
    /// 
    /// </summary>
    /// <param name="jsontext">Jsontext.</param>
	public MoShAnimationJSON(TextAsset jsontext)
    {
        betas = new float[10];
        LoadAnimationJSON(jsontext);
    }


    /// <summary>
    /// Load a new animation from a JSON TextAsset, replacing the values of the previous animation.
    /// </summary>
    /// <param name="jsontext">Jsontext.</param>
    public void LoadAnimationJSON (TextAsset jsontext) 
	{
        if (jsontext == null) {
            throw new Exception("Tried to instantiate Animation JSON with TextAsset == null");
        }
		JSONNode moshjson = JSON.Parse (jsontext.text);
		LoadAnimationJSON (moshjson);
	}


	public void LoadAnimationJSON(JSONNode moshjson)
	{
		// set gender. 
        JSONNode genderNode = moshjson[GenderKey];
        if (genderNode.IsNull) throw new Exception("File does not contain a gender field.");
        
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
        
        JSONNode fpsNode = moshjson[FPSKey];
        if (fpsNode.IsNull) throw new Exception("JSON has no fps field.");
        fps = fpsNode;
        SourceFPS = fps;

        
        length = moshjson[TransKey].Count;
        SourceLength = length;
        duration = length / (float)fps;

        for (int i = 0; i < 10; i++) {
            
            betas[i] = moshjson[BetasKey][i];
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
            float x, y, z;
            x = moshjson[TransKey][i][0];
            y = moshjson[TransKey][i][1];
            z = moshjson[TransKey][i][2];
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
                float qx, qy, qz, qw;
                // Arrrggg the error was that it was getting cast to an integer or something because I was multiplying by -1, not -1f.
                qx = -1.0f * moshjson[PosesKey][i][j][0];
                qy = moshjson[PosesKey][i][j][1];
                qz = moshjson[PosesKey][i][j][2];
                qw = -1.0f * moshjson[PosesKey][i][j][3];
                //Debug.Log("frame " + i + ": qx = " + qx + " qy = " + qy + "qz = " + qz + " qw = " + qw);
                Poses[i, j] = new Quaternion(qx, qy, qz, qw);

            }
        }
    }


}
