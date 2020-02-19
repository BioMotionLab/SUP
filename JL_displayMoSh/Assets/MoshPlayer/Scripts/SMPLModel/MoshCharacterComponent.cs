﻿using System;
using MoshPlayer.Scripts.Display;
using MoshPlayer.Scripts.Playback;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.SMPLModel {
    /// <summary>
    /// Altered version of loadMoshAnim from MPI. Allows a MoSh animation to be played at any time with a call to PlayAnim,
    /// rather than running on initialization of the script in the start method (requiring instantiating a new prefab.
    /// This is setup to handle loading a MoSh animation without prior knowledge of gender, swapping in the correct SMPLConstants model.
    /// </summary>
    [SelectionBase]
    public class MoshCharacterComponent : MonoBehaviour, MoshCharacter {
    
        MoshAnimation moshAnimation;
        Mesh          smplMeshClone;
        SkinnedMeshRenderer skinnedMeshRenderer;
        MoshMesh moshMesh;

        [SerializeField]
        // ReSharper disable once InconsistentNaming
        Gender gender = default;

        
        [SerializeField]
        // ReSharper disable once InconsistentNaming
        public bool setFeetOnGround = default;
        
        [SerializeField]
        // ReSharper disable once InconsistentNaming
        CharacterOptions options = default;

        public bool SetFeetOnGround => setFeetOnGround;
        public CharacterOptions Options => options;
        
        [SerializeField]
        // ReSharper disable once InconsistentNaming
        CharacterDisplayOptions displayOptions = default;

        public CharacterDisplayOptions DisplayOptions => displayOptions;
        public Gender Gender => gender;


        [FormerlySerializedAs("OffsetErrorBetweenPelvisAndZero")]
        [SerializeField]
        //new Vector3(.00217f,0.972724f,0.02858f);
        // ReSharper disable once InconsistentNaming
        Vector3 offsetErrorBetweenPelvisAndZero = default;

        public Vector3 OffsetErrorBetweenPelvisAndZero => offsetErrorBetweenPelvisAndZero;

        [FormerlySerializedAs("OffsetErrorInFbxBetweenRigAndMesh")]
        [SerializeField]
        // ReSharper disable once InconsistentNaming
        Vector3 offsetErrorInFbxBetweenRigAndMesh = default;

        public Vector3 OffsetErrorInFbxBetweenRigAndMesh => offsetErrorInFbxBetweenRigAndMesh;

        [SerializeField]
        // ReSharper disable once InconsistentNaming
        Vector3 combinedOffsets = default;

        void OnValidate() {
            combinedOffsets = offsetErrorBetweenPelvisAndZero - offsetErrorInFbxBetweenRigAndMesh;
        }

        public Vector3 CombinedOffset => combinedOffsets;

        Mesh originalMesh;
        
        [SerializeField]
        // ReSharper disable once InconsistentNaming
        ModelDefinition model = default;
        public ModelDefinition Model => model;
        
        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public SkinnedMeshRenderer SkinnedMeshRender => skinnedMeshRenderer;
        CharacterEvents events;
        
        public CharacterEvents Events {
            get {
                if (events == null) events = new CharacterEvents();
                return events;
            }
        }

        void OnEnable() {
            moshMesh = GetComponentInChildren<MoshMesh>();
            skinnedMeshRenderer = moshMesh.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer ==  null) throw new NullReferenceException("Can't find skinnedMeshRenderer in awake");
            
            //Create clone of mesh so original is not affected by any of our fiddling
            originalMesh = skinnedMeshRenderer.sharedMesh;
            skinnedMeshRenderer.sharedMesh = Instantiate( skinnedMeshRenderer.sharedMesh);

            PlaybackEventSystem.OnMeshDisplayStateChanged += MeshDisplayStateChanged;
            PlaybackEventSystem.OnBoneDisplayStateChanged += BoneDisplayStateChanged;
            PlaybackEventSystem.OnPointLightDisplayStateChanged += PointLightDisplayStateChanged;
            PlaybackEventSystem.OnChangeLivePoses += SetLivePoses;
            PlaybackEventSystem.OnChangeLivePoseBlendshapes += SetLivePoseBlendshapes;
            PlaybackEventSystem.OnChangeLiveBodyShape += SetLiveBodyShape;
            PlaybackEventSystem.OnChangeManualPosing += SetManualPosing;
        }

        void SetManualPosing(bool manualPosing) {
            options.AllowPoseManipulation = manualPosing;
        }

        void SetLiveBodyShape(bool liveBodyShape) {
            options.UpdateBodyShapeLive = liveBodyShape;
        }

        void SetLivePoseBlendshapes(bool livePoseBlendshapes) {
            options.UpdatePoseBlendshapesLive = livePoseBlendshapes;
        }

        void SetLivePoses(bool livePoses) {
            options.UpdatePosesLive = livePoses;
        }

        void PointLightDisplayStateChanged(PointLightDisplayState pointLightDisplayState) {
            displayOptions.DisplayPointLights = pointLightDisplayState;
        }

        void BoneDisplayStateChanged(BoneDisplayState boneDisplayState) {
            displayOptions.DisplayBones = boneDisplayState;
        }

        void OnDisable() {
            PlaybackEventSystem.OnMeshDisplayStateChanged += MeshDisplayStateChanged;
        }

        void MeshDisplayStateChanged(MeshDisplayState newState) {
            displayOptions.MeshDisplayState = newState;
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
        public void StartAnimation(MoshAnimation animationToStart, SettingsMain settings, PlaybackOptions playbackOptions) {
            moshAnimation = animationToStart;
            if (model.RotateToUnityCoords) RotateToUnityCoordinates();
        
            gameObject.SetActive(true);
            moshAnimation.AttachSkin(skinnedMeshRenderer, playbackOptions);
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
            Debug.Log($"{gameObject.name}'s Animation Complete");
            DestroyCharacter();
        }

        public void InterruptAnimation() {
            if (this == null || gameObject == null) return; // order of check important here. 
            
            //Debug.Log($"{gameObject.animationName}'s Animation Interrupted");
            DestroyCharacter();
        }

        void DestroyCharacter() {
            Destroy(gameObject);
        }
        
    }
}