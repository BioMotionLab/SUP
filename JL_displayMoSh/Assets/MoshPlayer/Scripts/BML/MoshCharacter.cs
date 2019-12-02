using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.BML {
    /// <summary>
    /// Altered version of loadMoshAnim from MPI. Allows a MoSh animation to be played at any time with a call to PlayAnim,
    /// rather than running on initialization of the script in the start method (requiring instantiating a new prefab.
    /// This is setup to handle loading a MoSh animation without prior knowledge of gender, swapping in the correct SMPLConstants model.
    /// </summary>
    [SelectionBase]
    public class MoshCharacter : MonoBehaviour {
    
        MoshAnimation moshAnimation;
        Mesh          smplMeshClone;
    
        public bool ChangeFrameRate = false;
    
        [FormerlySerializedAs("FrameRate")]
        public int DesiredFrameRate;

        SkinnedMeshRenderer skinnedMeshRenderer;
        [FormerlySerializedAs("MoshMesh2")]
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
            if (SMPLConstants.ZAxisUpInOriginalFiles) {
                transform.Rotate(-90f, 0f, 0f);
            }
        }
    
        /// <summary>
        /// Sets up and plays a mosh animation.
        /// </summary>
        public void StartAnimation(MoshAnimation animationToStart) {
            moshAnimation = animationToStart;
            ActivateMesh(moshAnimation.Gender);
        
            gameObject.SetActive(true);
            moshAnimation.AttachAnimationToMoshCharacter(skinnedMeshRenderer, Settings);
            if (ChangeFrameRate) moshAnimation.AdjustFrameRate(DesiredFrameRate);
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
            StopAnimation();
        }

        public void StopAnimation() {
            if (gameObject == null) return;
            Destroy(gameObject);
        }
    }
}