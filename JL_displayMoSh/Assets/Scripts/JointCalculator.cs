using System;
using UnityEngine;
using LightweightMatrixCSharp;

/// <summary>
/// Double singleton implementation of joint calculator.
/// Dongleton? Doubleton? Has two singleton instances.
/// </summary>
public class JointCalculator {
    // Note to self. Use Resources.FindObjectsOfTypeAll to check if there are memory leaks.

    static JointCalculator male = null;
    static JointCalculator female = null;

    Matrix[] _template;
    Matrix[] _jntsRegr; // doesn't need reset. 
    Vector3[] _joints;

    int nJoints = MoShAnimation.JointCount;
    int _numberOfJoints = MoShAnimation.JointCount;// = 24;
    int _numberOfBetas = MoShAnimation.BetaCount;// = 10;


    /// <summary>
    /// Get a reference to the male JointCalculator.
    /// </summary>
    public static JointCalculator Male {
        get {
            if (male == null) {
                TextAsset ta = Resources.Load<TextAsset>("jnt_regressors/betas2Jnt_unity_m");
                if (ta == null) {
                    Debug.LogError("Male text asset didn't load. Is null");
                }
                if (ta == null) {
                    throw new Exception("no male joint regressor file found");
                }
                male = new JointCalculator(ta);
                return male;
            }
            return male;
        }
    }

    /// <summary>
    /// Get a reference to the female JointCalculator.
    /// </summary>
    public static JointCalculator Female {
        get {
            if (female == null) {
                TextAsset ta = Resources.Load<TextAsset>("jnt_regressors/betas2Jnt_unity_f");
                if (ta == null) {
                    throw new Exception("no female joint regressor file found");
                }
                female = new JointCalculator(ta);
                return female;
            }
            return female;
        }
    }


    // private constructor. 
    JointCalculator(TextAsset jsontext) {
        _joints = new Vector3[nJoints];
        _template = new Matrix [3];
        _jntsRegr = new Matrix [3];

        for (int i = 0; i <= 2; i++) {
            _template[i] = new Matrix(nJoints, 1);
            _jntsRegr[i] = new Matrix(nJoints, _numberOfBetas);
        }

        SimpleJSON.JSONNode node = SimpleJSON.JSON.Parse(jsontext.text);

        for (int i = 0; i < nJoints; i++) {
            // Init joint template matrix
            double x = node["template_J"][i][0].AsDouble;
            double y = node["template_J"][i][1].AsDouble;
            double z = node["template_J"][i][2].AsDouble;

            (_template[0])[i, 0] = x;
            (_template[1])[i, 0] = y;
            (_template[2])[i, 0] = z;

            // Init beta regressor matrix    
            for (int j = 0; j < _numberOfBetas; j++) {
                (_jntsRegr[0])[i, j] = node["betasJ_regr"][i][0][j].AsDouble;
                (_jntsRegr[1])[i, j] = node["betasJ_regr"][i][1][j].AsDouble;
                (_jntsRegr[2])[i, j] = node["betasJ_regr"][i][2][j].AsDouble;
            }
        }
    }

    /// <summary>
    /// Calculate initial joint positions from betas. 
    /// </summary>
    /// <returns>The joints.</returns>
    public Vector3[] calculateJoints(float[] betas) {
        // Create beta value matrix
        Matrix betaMatrix = new Matrix(_numberOfBetas, 1); // column vector.
        for (int row = 0; row < _numberOfBetas; row++) {
            betaMatrix[row, 0] = betas[row];
        }

        Matrix newJointsX = _jntsRegr[0] * betaMatrix + _template[0];
        Matrix newJointsY = _jntsRegr[1] * betaMatrix + _template[1];
        Matrix newJointsZ = _jntsRegr[2] * betaMatrix + _template[2];

        // Update joints vector
        for (int row = 0; row < _numberOfJoints; row++) {
            // Convert Maya regressor to Unity coordinate system by negating X value
            _joints[row] = new Vector3(-(float)newJointsX[row, 0], (float)newJointsY[row, 0], (float)newJointsZ[row, 0]);
        }
        return _joints;
    }


    public static Vector3[] GetDefaultJoints(Gender g) {
        JointCalculator jc;
        if (g == Gender.MALE) {
            jc = JointCalculator.Male;
        } else {
            jc = JointCalculator.Female;
        }
        float[] b0 = new float[10];
        return jc.calculateJoints(b0);
    }


}