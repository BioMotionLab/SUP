using System;
using MoshPlayer.Scripts.Playback;
using UnityEngine;

namespace MoshPlayer.Scripts.SMPLModel {
    /// <summary>
    /// Altered version of loadMoshAnim from MPI. Allows a MoSh animation to be played at any time with a call to PlayAnim,
    /// rather than running on initialization of the script in the start method (requiring instantiating a new prefab.
    /// This is setup to handle loading a MoSh animation without prior knowledge of gender, swapping in the correct SMPLConstants model.
    /// </summary>
    [SelectionBase]
    public class MoshCharacterComponent : MonoBehaviour, MoshCharacter {
        
        [SerializeField]
        Gender gender = default;
        public Gender Gender => gender;

        [SerializeField]
        CharacterRenderOptions characterRenderOptions = default;
        public CharacterRenderOptions RenderOptions => characterRenderOptions;
        
        [SerializeField]
        CharacterDisplayOptions characterDisplayOptions = default;
        public CharacterDisplayOptions DisplayOptions => characterDisplayOptions;
        
        [SerializeField]
        Vector3 offsetErrorBetweenPelvisAndZero = default;
        public Vector3 OffsetErrorBetweenPelvisAndZero => offsetErrorBetweenPelvisAndZero;

        [SerializeField]
        Vector3 offsetErrorInFbxBetweenRigAndMesh = default;
        public Vector3 OffsetErrorInFbxBetweenRigAndMesh => offsetErrorInFbxBetweenRigAndMesh;

        [SerializeField]
        Vector3 combinedOffsets = default;
        public Vector3 CombinedOffset => combinedOffsets;
        
        [SerializeField]
        ModelDefinition model = default;
        public ModelDefinition Model => model;


        public IndividualizedBody Body => moshAnimation?.Body;

        Mesh originalMesh;
        MoshAnimation moshAnimation;
        MoshMesh moshMesh;
        
        SkinnedMeshRenderer skinnedMeshRenderer;
        public SkinnedMeshRenderer SkinnedMeshRender => skinnedMeshRenderer;

        CharacterEvents events;
        public CharacterEvents Events => events ?? (events = new CharacterEvents());

        void OnValidate() {
            combinedOffsets = offsetErrorBetweenPelvisAndZero - offsetErrorInFbxBetweenRigAndMesh;
        }

        void OnEnable() {
            moshMesh = GetComponentInChildren<MoshMesh>();
            skinnedMeshRenderer = moshMesh.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer ==  null) throw new NullReferenceException("Can't find skinnedMeshRenderer in awake");
            
            //Create clone of mesh so original is not affected by any of our fiddling
            originalMesh = skinnedMeshRenderer.sharedMesh;
            skinnedMeshRenderer.sharedMesh = Instantiate( originalMesh);
        }

        void OnDestroy() {
            skinnedMeshRenderer.sharedMesh = originalMesh;
            //Debug.Log("Resetting mesh to previous state on destroy");
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
        public void StartAnimation(MoshAnimation animationToStart, PlaybackOptions playbackOptions, CharacterDisplayOptions displayOptions, CharacterRenderOptions renderOptions) {
            characterRenderOptions = renderOptions;
            characterDisplayOptions = displayOptions;
            
            moshAnimation = animationToStart;
            if (model.RotateToUnityCoords) RotateToUnityCoordinates();
        
            gameObject.SetActive(true);
            moshAnimation.AttachSkin(skinnedMeshRenderer);
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

        void StopAnimation() {
            if (gameObject == null) return;
            Debug.Log($"\t{gameObject.name}'s Animation Complete");
            DestroyCharacter();
        }

        public void InterruptAnimation() {
            if (this == null || gameObject == null) return; // order of check important here. 
            moshAnimation.End();
            
            //Debug.Log($"{gameObject.animationName}'s Animation Interrupted");
            DestroyCharacter();
        }

        void DestroyCharacter() {
            Destroy(gameObject);
        }
        
    }
}