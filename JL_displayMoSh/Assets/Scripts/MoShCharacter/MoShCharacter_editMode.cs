
#undef DANGERMODE

using UnityEngine;

public partial class MoShCharacter : MonoBehaviour 
{

    // unfortuneately conditional compilation doesn't actually work to determine
    // between editor edit mode and editor play mode.
    // #if UNITY_EDITOR

#if DANGERMODE
    [Range(-5f, 5f)]
    public float[] editorBetas = new float[10];

    public void EditorComputeShapePose_male()
    {
        Debug.Log(meshCloned_m);
        Debug.Log(meshCloned_f);
        activateMesh(Genders.MALE);
        float[] values = new float[20];
        for (int i = 0; i < 10; i++) {
            if (editorBetas[i] >= 0) {
                values[2 * i] = editorBetas[i] * 100f / 5f;
                values[(2 * i) + 1] = 0f;
            }
            else {
                values[2 * i] = 0f;
                values[(2 * i) + 1] = -1 * editorBetas[i] * 100f / 5f;
            }
        }
        SkinnedMeshRenderer mr = GetComponent<SkinnedMeshRenderer>();
        mr.sharedMesh = smpl_m;
        for (int i = 0; i < 20; i++) {
            mr.SetBlendShapeWeight(i, values[i]);
        }
        JointCalculator jc = JointCalculator.Male;
        BMLModifyBones bm = new BMLModifyBones(mr);
        Vector3[] joints = jc.calculateJoints(editorBetas);
        bm.updateBonePositions(joints, false);
    }


    public void EditorComputeShapePose_female()
    {
        Debug.Log(meshCloned_m);
        Debug.Log(meshCloned_f);
        activateMesh(Genders.FEMALE);
        float[] values = new float[20];
        for (int i = 0; i < 10; i++) {
            if (editorBetas[i] >= 0) {
                values[2 * i] = editorBetas[i] * 100f / 5f;
                values[(2 * i) + 1] = 0f;
            }
            else {
                values[2 * i] = 0f;
                values[(2 * i) + 1] = -1 * editorBetas[i] * 100f / 5f;
            }
        }
        mr.sharedMesh = smpl_f;
        for (int i = 0; i < 20; i++) {
            mr.SetBlendShapeWeight(i, values[i]);
        }


        JointCalculator jc = JointCalculator.Female;
        BMLModifyBones bm = new BMLModifyBones(mr);
        Vector3[] joints = jc.calculateJoints(editorBetas);
        bm.updateBonePositions(joints, false);
    }


    public void EditorSetDefaultBones_female()
    {
        activateMesh(Genders.FEMALE);

        // floats in general default to 0f
        float[] zerobetas = new float[10];
        JointCalculator jc = JointCalculator.Female;
        SkinnedMeshRenderer mr = GetComponent<SkinnedMeshRenderer>();
        mr.sharedMesh = smpl_f;
        BMLModifyBones bm = new BMLModifyBones(mr);
        Vector3[] joints = jc.calculateJoints(zerobetas);
        bm.updateBonePositions(joints);
    }


    public void EditorSetDefaultBones_male()
    {
        Debug.Log(meshCloned_m);
        Debug.Log(meshCloned_f);
        activateMesh(Genders.MALE);
        // floats default to 0f
        float[] localbetas = new float[10];
        JointCalculator jc = JointCalculator.Male;
        SkinnedMeshRenderer mr = GetComponent<SkinnedMeshRenderer>();
        mr.sharedMesh = smpl_m;

        BMLModifyBones bm = new BMLModifyBones(mr);
        Vector3[] joints = jc.calculateJoints(localbetas);
        //bm.updateBonePositions(joints, false);
        bm.updateBonePositions(joints);
    }
#endif

}
