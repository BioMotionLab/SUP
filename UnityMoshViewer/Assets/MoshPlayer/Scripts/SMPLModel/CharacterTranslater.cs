using System;
using MoshPlayer.Scripts.Playback;
using UnityEngine;

namespace MoshPlayer.Scripts.SMPLModel {
    
    public class CharacterTranslater : MonoBehaviour {
        [SerializeField] Vector3 translation;
        [SerializeField] Vector3 firstFrameTranslation;
        
        [SerializeField] Grounder grounder;
        
        MoshCharacter       moshCharacter;
        SkinnedMeshRenderer skinnedMeshRenderer;

        bool firstFrame = false;
        bool bodyChanged = false;
        
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
            firstFrameTranslation = translation;
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
            translation = trans;
            //Debug.Log($"setting trans: {translation.ToString("F4")}");
        }

        void UpdateTranslation() {
            
            if(moshCharacter.RenderOptions.AllowPoseManipulation) return;
            
            Vector3 finalTrans = Vector3.zero;

            finalTrans = UpdateVerticalTranslation(finalTrans);
            finalTrans = UpdateHorizontalTranslation(finalTrans);

            moshCharacter.gameObject.transform.localPosition = finalTrans;
            
        }
        
        
        
        Vector3 UpdateVerticalTranslation(Vector3 finalTrans) {
            //height needs to be dealt with separately because of ground-snapping
            if (moshCharacter.RenderOptions.UpdateTranslationLiveY && moshCharacter.RenderOptions.UpdatePosesLive)
                finalTrans.y = translation.y;
            else
                finalTrans.y = firstFrameTranslation.y;

            finalTrans = grounder.ApplyGround(finalTrans, firstFrame);
            return finalTrans;
        }

        Vector3 UpdateHorizontalTranslation(Vector3 finalTrans) {
            if (moshCharacter.RenderOptions.UpdatePosesLive) {
                //Horizontal plane simple enough
                finalTrans.x = moshCharacter.RenderOptions.UpdateTranslationLiveXZ ? translation.x : firstFrameTranslation.x;
                finalTrans.z = moshCharacter.RenderOptions.UpdateTranslationLiveXZ ? translation.z : firstFrameTranslation.z;
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
    }
}