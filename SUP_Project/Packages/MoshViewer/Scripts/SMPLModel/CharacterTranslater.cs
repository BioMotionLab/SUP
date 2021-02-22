using System;
using MoshPlayer.Scripts.Playback;
using UnityEngine;

namespace MoshPlayer.Scripts.SMPLModel {
    
    public class CharacterTranslater : MonoBehaviour {
        [SerializeField] Vector3 currentTranslation;
        Vector3 firstFrameTranslation;
        
        [SerializeField] Grounder grounder;
        
        MoshCharacter       moshCharacter;
        SkinnedMeshRenderer skinnedMeshRenderer;

        bool firstFrame = false;
        bool bodyChanged = false;

        PlaybackSettings playbackSettings;
        int index;

        void Awake() {
            moshCharacter = GetComponent<MoshCharacter>();
            if (moshCharacter == null) throw new NullReferenceException("Can't find MoshCharacter component");

            skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer == null) throw new NullReferenceException("Can't find SkinnedMeshRenderer component");

            grounder = new Grounder(moshCharacter, skinnedMeshRenderer);
            
            moshCharacter.Events.OnBodyChanged += BodyChanged;
            PlaybackEventSystem.OnChangeSnapToGround += GroundingChanged;
        }

        void OnDestroy() {
            grounder.Destory();
            moshCharacter.Events.OnBodyChanged -= BodyChanged;
            PlaybackEventSystem.OnChangeSnapToGround -= GroundingChanged;
        }

        void Update() {
            UpdateTranslation();
            
            if (firstFrame) ConfigureFirstFrame();
            

            if (!moshCharacter.RenderOptions.UpdatePosesLive) UpdateFootOffset();
            else if (!firstFrame && bodyChanged) UpdateFootOffset();
        }
        
        void ConfigureFirstFrame() {
            firstFrameTranslation = currentTranslation;
            grounder.InitGround();
            bodyChanged = false;
            firstFrame = false;
            UpdateTranslation();
        }
        
        
        void UpdateFootOffset() {
            bodyChanged = false;
            grounder.UpdateGround();
            UpdateTranslation();
        }

        void GroundingChanged(GroundSnapType unused) {
            UpdateTranslation();
        }
        
        public void SetTranslation(Vector3 trans) {
            currentTranslation = trans;
            //Debug.Log($"setting trans: {translation.ToString("F4")}");
        }

        void UpdateTranslation() {
            
            if(moshCharacter.RenderOptions.AllowPoseManipulation) return;
            
            Vector3 finalTrans = Vector3.zero;

            finalTrans = UpdateVerticalTranslation(finalTrans);
            finalTrans = UpdateHorizontalTranslation(finalTrans);

            finalTrans += GetOffsetFromIndex();

            moshCharacter.gameObject.transform.localPosition = finalTrans;
            
        }
        
        
        
        Vector3 UpdateVerticalTranslation(Vector3 finalTrans) {
            //height needs to be dealt with separately because of ground-snapping
            if (moshCharacter.RenderOptions.UpdateTranslationLiveY && moshCharacter.RenderOptions.UpdatePosesLive)
                finalTrans.y = currentTranslation.y;
            else
                finalTrans.y = firstFrameTranslation.y;

            finalTrans = grounder.ApplyGround(finalTrans, firstFrame);
            return finalTrans;
        }

        Vector3 UpdateHorizontalTranslation(Vector3 finalTrans) {
            if (moshCharacter.RenderOptions.UpdatePosesLive) {
                //Horizontal plane simple enough
                finalTrans.x = moshCharacter.RenderOptions.UpdateTranslationLiveXZ ? currentTranslation.x : firstFrameTranslation.x;
                finalTrans.z = moshCharacter.RenderOptions.UpdateTranslationLiveXZ ? currentTranslation.z : firstFrameTranslation.z;
            }

            return finalTrans;
        }
        
        public void NotifyFirstFrame() {
            firstFrame = true;
        }
        
        public void ForceUpdate() {
            Update();
        }
        
        void BodyChanged() {
            bodyChanged = true;
        }

        public void AddOffset(int animationIndex) {
            this.index = animationIndex;
        }

        Vector3 GetOffsetFromIndex() {
            if (playbackSettings == null) return Vector3.zero;
            Vector3 offsetSpacing = playbackSettings.OffSetSpacing;
            Vector3 translationOffset = new Vector3(offsetSpacing.x * index, offsetSpacing.y * index,
                offsetSpacing.z * index);
            if (index % 2 > 0)
                translationOffset =
                    -1 * (translationOffset - new Vector3(-offsetSpacing.x, -offsetSpacing.y, -offsetSpacing.z));
            return translationOffset;
        }

        public void SetPlaybackOptions(PlaybackSettings playbackSettings) {
            this.playbackSettings = playbackSettings;
        }
    }
}