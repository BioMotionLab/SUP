
using UnityEngine;
using System;
using UnityEngine.Serialization;

/// <summary>
/// Altered version of loadMoshAnim from MPI. Allows a MoSh animation to be played at any time with a call to PlayAnim,
/// rather than running on initialization of the script in the start method (requiring instantiating a new prefab.
/// This is setup to handle loading a MoSh animation without prior knowledge of gender, swapping in the correct SMPL model.
/// </summary>
[SelectionBase]
public class MoshCharacter : MonoBehaviour {
    
    MoshAnimation moshAnimation;
    Mesh smplMeshClone;
    
    public bool ChangeFrameRate = false;
    
    [FormerlySerializedAs("FrameRate")]
    public int DesiredFrameRate;

    SkinnedMeshRenderer skinnedMeshRenderer;
    [FormerlySerializedAs("moshMesh")]
    [SerializeField]
    MoshMesh MoshMesh = default;
    [SerializeField]
    SMPLSettings Settings = default;

    void Awake() {
        skinnedMeshRenderer = MoshMesh.GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer ==  null) throw new NullReferenceException("Can't find skinnedMeshRenderer in awake");
        RotateToUnityCoordinatesIfNeeded();
    }

    /// <summary>
    /// Rotate the FBX model in case up direction is not Y - axis;
    /// JL: this seems weird. 
    /// </summary>
    void RotateToUnityCoordinatesIfNeeded() {
        if (SMPL.ZAxisUpInOriginalFiles) {
            transform.Rotate(-90f, 0f, 0f);
        }
    }
    
    /// <summary>
    /// Sets up and plays a mosh animation.
    /// </summary>
    public void StartAnimation(MoshAnimation animationToStart) {
        Debug.Log($"starting animation for {name}");
        moshAnimation = animationToStart;
        ActivateMesh(moshAnimation.Gender);
        
        gameObject.SetActive(true);
        moshAnimation.AttachAnimationToMoshCharacter(skinnedMeshRenderer);
        if (ChangeFrameRate) moshAnimation.AdjustFrameRate(DesiredFrameRate);
        Debug.Log($"started animation for {name}");
    }
    
    void Update() {
        UpdateAnimation();
    }

    void UpdateAnimation() {
        if (moshAnimation == null) return;
        if (moshAnimation.Finished) {
            AnimationCompleted();
            return;
        }
        moshAnimation.PlayCurrentFrame();
    }

    void ActivateMesh(Gender gender) {
        skinnedMeshRenderer.sharedMesh = Instantiate(Settings.GetMeshPrefab(gender));
    }

    void AnimationCompleted() {
        Debug.Log("animation complete");
        Destroy(gameObject);
    }
}