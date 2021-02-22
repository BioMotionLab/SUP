using System;
using Display;
using MoshPlayer;
using Playback;
using UnityEngine;
using UnityEngine.Serialization;

namespace SMPLModel {
    /// <summary>
    /// Altered version of loadMoshAnim from MPI. Allows a MoSh animation to be played at any time with a call to PlayAnim,
    /// rather than running on initialization of the script in the start method (requiring instantiating a new prefab.
    /// This is setup to handle loading a MoSh animation without prior knowledge of gender, swapping in the correct SMPLConstants model.
    /// </summary>
    [SelectionBase]
    public class CharacterComponent : MonoBehaviour, MoshCharacter {
          
        
        [SerializeField]
        ModelDefinition model = default;
        public ModelDefinition Model => model;
        
        
        [SerializeField]
        Gender gender = default;
        public Gender Gender => gender;

        
        [SerializeField]
        BodyOptions bodyOptions = default;
        public BodyOptions RenderOptions => bodyOptions;
        
        [FormerlySerializedAs("characterSetings")]
        [FormerlySerializedAs("characterDisplayOptions")] 
        [SerializeField]
        DisplaySettings displaySetings = default;
        public DisplaySettings DisplaySettings => displaySetings;


        [SerializeField] MeshCorrection meshCorrection = default;
        public MeshCorrection MeshCorrection => meshCorrection;
      


        public IndividualizedBody Body => moshAnimation?.Body;

        Mesh originalMesh;
        MoshAnimation moshAnimation;
        MoshMesh moshMesh;
        
        SkinnedMeshRenderer skinnedMeshRenderer;
        public SkinnedMeshRenderer SkinnedMeshRender => skinnedMeshRenderer;

        CharacterEvents events;
        int animationIndex;
        public CharacterEvents Events => events ?? (events = new CharacterEvents());

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
        public void StartAnimation(MoshAnimation animationToStart, PlaybackSettings playbackSettings, DisplaySettings characterSettings, BodyOptions renderOptions) {
            bodyOptions = renderOptions;
            displaySetings = characterSettings;
            moshAnimation = animationToStart;
            if (model.RotateToUnityCoords) RotateToUnityCoordinates();
        
            gameObject.SetActive(true);
            moshAnimation.AttachSkin(skinnedMeshRenderer);
            UpdateAnimation();

            SetOffsetsFromIndex(playbackSettings);
            SetMaterialFromIndex(characterSettings.MeshDisplaySettings);
        }

        void SetOffsetsFromIndex(PlaybackSettings playbackSettings) {
            if (playbackSettings.OffsetMultipleAnimations) {
                CharacterTranslater translater = GetComponent<CharacterTranslater>();
                translater.SetPlaybackOptions(playbackSettings);
                translater.AddOffset(animationIndex);
            }
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

        public void SetIndex(int animationIndex) {
            this.animationIndex = animationIndex;
        }

        void SetMaterialFromIndex(MeshDisplaySettings displayOptions) {
            //Debug.Log($"index: {animationIndex}, count:{displayOptions.OptionalMaterialList.Count}");
            if (animationIndex < displayOptions.OptionalMaterialList.Count) {
                MeshDisplay meshDisplay = GetComponent<MeshDisplay>();
               meshDisplay.overwriteMaterial = displayOptions.OptionalMaterialList[animationIndex];
            }
        }

        void DestroyCharacter() {
            Destroy(gameObject);
        }
        
    }
}