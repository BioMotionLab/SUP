using Display;
using Playback;
using Settings;

namespace SMPLModel {

    public interface Viewer {
        BodyOptions RuntimeBodyOptions { get; }
        DisplaySettings RuntimeDisplaySettings { get; }
        PlaybackSettings RuntimePlaybackSettings { get; }
    }
    
    public class UserModifiedSettingsHandler {
        Viewer viewer;

        public UserModifiedSettingsHandler(Viewer viewer) {
            this.viewer = viewer;
            
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
            viewer.RuntimeBodyOptions.UpdateTranslationLiveY = changeUpdateYTranslation;
        }

        void SetUpdateXzTranslation(bool changeUpdateXzTranslation) {
            viewer.RuntimeBodyOptions.UpdateTranslationLiveXZ = changeUpdateXzTranslation;
        }

        void SetManualPosing(bool manualPosing) {
            viewer.RuntimeBodyOptions.AllowPoseManipulation = manualPosing;
        }

        void SetLiveBodyShape(bool liveBodyShape) {
            viewer.RuntimeBodyOptions.UpdateBodyShapeLive = liveBodyShape;
        }

        void SetLivePoseBlendshapes(bool livePoseBlendshapes) {
            viewer.RuntimeBodyOptions.UpdatePoseBlendshapesLive = livePoseBlendshapes;
        }

        void SetLivePoses(bool livePoses) {
            viewer.RuntimeBodyOptions.UpdatePosesLive = livePoses;
        }

        void PointLightDisplayStateChanged(PointLightDisplayState pointLightDisplayState) {
            viewer.RuntimeDisplaySettings.DisplayPointLights = pointLightDisplayState;
        }

        void BoneDisplayStateChanged(BoneDisplayState boneDisplayState) {
            viewer.RuntimeDisplaySettings.DisplayBones = boneDisplayState;
        }

        void MeshDisplayStateChanged(MeshDisplayState newState) {
            viewer.RuntimeDisplaySettings.DisplayMeshAs = newState;
        }

        void SetIndividualizedBodyState(bool newState) {
            viewer.RuntimeBodyOptions.ShowIndividualizedBody = newState;
        }

        void SetLoopState(bool state) {
            viewer.RuntimePlaybackSettings.Loop = state;
        }

        void SetSnapToGround(GroundSnapType snaptype) {
            viewer.RuntimeBodyOptions.GroundSnap = snaptype;
        }
    }
}