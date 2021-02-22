using Display;
using MoshPlayer;
using Playback;

namespace SMPLModel {

    public interface Viewer {
        BodyOptions RuntimeBodyOptions { get; }
        DisplaySettings RuntimeDisplaySettings { get; }
        PlaybackSettings RuntimePlaybackSettings { get; }
    }
    
    public class UserModifiedSettingsHandler {
        Viewer moshViewer;

        public UserModifiedSettingsHandler(Viewer moshViewer) {
            this.moshViewer = moshViewer;
            
            PlaybackEventSystem.OnMeshDisplayStateChanged += MeshDisplayStateChanged;
            PlaybackEventSystem.OnBoneDisplayStateChanged += BoneDisplayStateChanged;
            PlaybackEventSystem.OnPointLightDisplayStateChanged += PointLightDisplayStateChanged;
            PlaybackEventSystem.OnChangeLivePoses += SetLivePoses;
            PlaybackEventSystem.OnChangeLivePoseBlendshapes += SetLivePoseBlendshapes;
            PlaybackEventSystem.OnChangeLiveBodyShape += SetLiveBodyShape;
            PlaybackEventSystem.OnChangeManualPosing += SetManualPosing;
            PlaybackEventSystem.OnChangeSnapToGround += SetSnapToGround;
            PlaybackEventSystem.OnChangeUpdateYTranslation += SetUpdateYTranslation;
            PlaybackEventSystem.OnChangeUpdateXzTranslation += SetUpdateXzTranslation;
            PlaybackEventSystem.OnChangeIndividualizedBody += SetIndividualizedBodyState;
            PlaybackEventSystem.OnChangeLoopState += SetLoopState;
        }

        public void Destroy() {
            PlaybackEventSystem.OnMeshDisplayStateChanged -= MeshDisplayStateChanged;
            PlaybackEventSystem.OnBoneDisplayStateChanged -= BoneDisplayStateChanged;
            PlaybackEventSystem.OnPointLightDisplayStateChanged += PointLightDisplayStateChanged;
            PlaybackEventSystem.OnChangeLivePoses -= SetLivePoses;
            PlaybackEventSystem.OnChangeLivePoseBlendshapes -= SetLivePoseBlendshapes;
            PlaybackEventSystem.OnChangeLiveBodyShape -= SetLiveBodyShape;
            PlaybackEventSystem.OnChangeManualPosing -= SetManualPosing;
            PlaybackEventSystem.OnChangeSnapToGround -= SetSnapToGround;
            PlaybackEventSystem.OnChangeUpdateYTranslation -= SetUpdateYTranslation;
            PlaybackEventSystem.OnChangeUpdateXzTranslation -= SetUpdateXzTranslation;
            PlaybackEventSystem.OnChangeIndividualizedBody -= SetIndividualizedBodyState;
            PlaybackEventSystem.OnChangeLoopState -= SetLoopState;
        }

        void SetUpdateYTranslation(bool changeUpdateYTranslation) {
            moshViewer.RuntimeBodyOptions.UpdateTranslationLiveY = changeUpdateYTranslation;
        }

        void SetUpdateXzTranslation(bool changeUpdateXzTranslation) {
            moshViewer.RuntimeBodyOptions.UpdateTranslationLiveXZ = changeUpdateXzTranslation;
        }

        void SetManualPosing(bool manualPosing) {
            moshViewer.RuntimeBodyOptions.AllowPoseManipulation = manualPosing;
        }

        void SetLiveBodyShape(bool liveBodyShape) {
            moshViewer.RuntimeBodyOptions.UpdateBodyShapeLive = liveBodyShape;
        }

        void SetLivePoseBlendshapes(bool livePoseBlendshapes) {
            moshViewer.RuntimeBodyOptions.UpdatePoseBlendshapesLive = livePoseBlendshapes;
        }

        void SetLivePoses(bool livePoses) {
            moshViewer.RuntimeBodyOptions.UpdatePosesLive = livePoses;
        }

        void PointLightDisplayStateChanged(PointLightDisplayState pointLightDisplayState) {
            moshViewer.RuntimeDisplaySettings.DisplayPointLights = pointLightDisplayState;
        }

        void BoneDisplayStateChanged(BoneDisplayState boneDisplayState) {
            moshViewer.RuntimeDisplaySettings.DisplayBones = boneDisplayState;
        }

        void MeshDisplayStateChanged(MeshDisplayState newState) {
            moshViewer.RuntimeDisplaySettings.DisplayMeshAs = newState;
        }

        void SetIndividualizedBodyState(bool newState) {
            moshViewer.RuntimeBodyOptions.ShowIndividualizedBody = newState;
        }

        void SetLoopState(bool state) {
            moshViewer.RuntimePlaybackSettings.Loop = state;
        }

        void SetSnapToGround(GroundSnapType snaptype) {
            moshViewer.RuntimeBodyOptions.GroundSnap = snaptype;
        }
    }
}