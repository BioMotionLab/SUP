using MoshPlayer.Scripts.Display;
using MoshPlayer.Scripts.Playback;

namespace MoshPlayer.Scripts.SMPLModel {
    public class UserModifiedSettingsHandler {
        MoshViewerComponent moshViewerComponent;

        public UserModifiedSettingsHandler(MoshViewerComponent moshViewerComponent) {
            this.moshViewerComponent = moshViewerComponent;
            
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
            moshViewerComponent.RuntimeBodyOptions.UpdateTranslationLiveY = changeUpdateYTranslation;
        }

        void SetUpdateXzTranslation(bool changeUpdateXzTranslation) {
            moshViewerComponent.RuntimeBodyOptions.UpdateTranslationLiveXZ = changeUpdateXzTranslation;
        }

        void SetManualPosing(bool manualPosing) {
            moshViewerComponent.RuntimeBodyOptions.AllowPoseManipulation = manualPosing;
        }

        void SetLiveBodyShape(bool liveBodyShape) {
            moshViewerComponent.RuntimeBodyOptions.UpdateBodyShapeLive = liveBodyShape;
        }

        void SetLivePoseBlendshapes(bool livePoseBlendshapes) {
            moshViewerComponent.RuntimeBodyOptions.UpdatePoseBlendshapesLive = livePoseBlendshapes;
        }

        void SetLivePoses(bool livePoses) {
            moshViewerComponent.RuntimeBodyOptions.UpdatePosesLive = livePoses;
        }

        void PointLightDisplayStateChanged(PointLightDisplayState pointLightDisplayState) {
            moshViewerComponent.RuntimeDisplaySettings.DisplayPointLights = pointLightDisplayState;
        }

        void BoneDisplayStateChanged(BoneDisplayState boneDisplayState) {
            moshViewerComponent.RuntimeDisplaySettings.DisplayBones = boneDisplayState;
        }

        void MeshDisplayStateChanged(MeshDisplayState newState) {
            moshViewerComponent.RuntimeDisplaySettings.DisplayMeshAs = newState;
        }

        void SetIndividualizedBodyState(bool newState) {
            moshViewerComponent.RuntimeBodyOptions.ShowIndividualizedBody = newState;
        }

        void SetLoopState(bool state) {
            moshViewerComponent.RuntimePlaybackSettings.Loop = state;
        }

        void SetSnapToGround(GroundSnapType snaptype) {
            moshViewerComponent.RuntimeBodyOptions.GroundSnap = snaptype;
        }
    }
}