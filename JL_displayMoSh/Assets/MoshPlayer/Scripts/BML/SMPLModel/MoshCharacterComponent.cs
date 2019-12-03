using System;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    /// <summary>
    /// Altered version of loadMoshAnim from MPI. Allows a MoSh animation to be played at any time with a call to PlayAnim,
    /// rather than running on initialization of the script in the start method (requiring instantiating a new prefab.
    /// This is setup to handle loading a MoSh animation without prior knowledge of gender, swapping in the correct SMPLConstants model.
    /// </summary>
    [SelectionBase]
    public class MoshCharacterComponent : MonoBehaviour, MoshCharacter {
    
        MoshAnimation moshAnimation;
        Mesh          smplMeshClone;
    
        public bool ChangeFrameRate = false;
        public int DesiredFrameRate;
        SkinnedMeshRenderer skinnedMeshRenderer;
        
        
        [SerializeField]
        MoshMesh MoshMesh = default;

        [SerializeField]
        // ReSharper disable once InconsistentNaming
        SMPLSettings settings = default;

        // ReSharper disable once ConvertToAutoPropertyWhenPossible
        public SMPLSettings Settings => settings;
        
        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public SkinnedMeshRenderer SkinnedMeshRender => skinnedMeshRenderer;
        

        void Awake() {
            skinnedMeshRenderer = MoshMesh.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer ==  null) throw new NullReferenceException("Can't find skinnedMeshRenderer in awake");
            RotateToUnityCoordinates();
        }

        /// <summary>
        /// Rotate the FBX model in case up direction is not Y - axis;
        /// JL: this seems weird. 
        /// </summary>
        void RotateToUnityCoordinates() {
            transform.Rotate(-90f, 0f, 0f);
            
        }
    
        /// <summary>
        /// Sets up and plays a mosh animation.
        /// </summary>
        public void StartAnimation(MoshAnimation animationToStart) {
            moshAnimation = animationToStart;
            ActivateMesh(moshAnimation.Gender);
        
            gameObject.SetActive(true);
            moshAnimation.AttachAnimationToMoshCharacter(skinnedMeshRenderer, settings);
            if (ChangeFrameRate) moshAnimation.AdjustFrameRate(DesiredFrameRate);
            UpdateAnimation();
        }
    
        void Update() {
            UpdateAnimation();
        }

        void UpdateAnimation() {
            if (moshAnimation == null) return;
            if (moshAnimation.Finished) {
                StopAnimation();
                return;
            }
            moshAnimation.PlayCurrentFrame();
        }

        void ActivateMesh(Gender gender) {
            skinnedMeshRenderer.sharedMesh = Instantiate(settings.GetMeshPrefab(gender));
        }

        void StopAnimation() {
            if (gameObject == null) return;
            //Debug.Log($"{gameObject.name}'s Animation Complete");
            DestroyCharacter();
        }

        public void InterruptAnimation() {
            if (gameObject == null) return;
            //Debug.Log($"{gameObject.name}'s Animation Interrupted");
            DestroyCharacter();
        }

        void DestroyCharacter() {
            Destroy(gameObject);
        }
    }
}