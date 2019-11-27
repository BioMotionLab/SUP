
using UnityEngine;
using System;
using UnityEngine.Serialization;

/// <summary>
/// Altered version of loadMoshAnim from MPI. Allows a MoSh animation to be played at any time with a call to PlayAnim,
/// rather than running on initialization of the script in the start method (requiring instantiating a new prefab.
/// This is setup to handle loading a MoSh animation without prior knowledge of gender, swapping in the correct SMPL model.
/// </summary>
[SelectionBase]
[RequireComponent(typeof(SkinnedMeshRenderer))]
public class MoshCharacter : MonoBehaviour {
    
    MoshAnimation moshAnimation;
    Mesh smplMeshClone;
    
    public bool ChangeFrameRate = false;
    
    [FormerlySerializedAs("FrameRate")]
    public int DesiredFrameRate;

    [FormerlySerializedAs("meshRenderer")]
    public SkinnedMeshRenderer MeshRenderer;
    
    
    [SerializeField]
    SMPLSettings Settings = default;

    void Awake()
    {
        MeshRenderer = GetComponent<SkinnedMeshRenderer>();
        
        RotateToUnityCoordinatesIfNeeded();
    }

    void RotateToUnityCoordinatesIfNeeded() {
        //Rotate the FBX model in case up direction is not Y - axis;
        // this seems weird. 
        if (SMPL.ZAxisUpInOriginalFiles) {
            transform.parent.Rotate(-90f, 0f, 0f);
        }
    }
    
    /// <summary>
    /// Sets up and plays a mosh animation.
    /// This is the main point of interaction with the load MoSh functionality.
    /// Give it a file, call the method and it will do the whole thing.
    /// </summary>
    public void StartAnimation(String jsonAnimationFileWholeString)
    {
        transform.parent.gameObject.SetActive(true);
        moshAnimation = new MoShAnimationFromJSON(jsonAnimationFileWholeString).Build();
        moshAnimation.AttachAnimationToMoshCharacter(this);
    }
    

    void Update() {
        if (moshAnimation.Finished) return;
        moshAnimation.PlayCurrentFrame();
    }
    
    public void ActivateMesh(Gender gender) {
        MeshRenderer.sharedMesh = Instantiate(Settings.GetMeshPrefab(gender));
    }

    public void AnimationCompleted() {
        if (Settings.HideMeshWhenFinished) transform.parent.gameObject.SetActive(false);
    }
}