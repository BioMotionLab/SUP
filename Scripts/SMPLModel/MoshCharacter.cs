using MoshPlayer.Scripts.Playback;
using UnityEngine;

namespace MoshPlayer.Scripts.SMPLModel {
    public interface MoshCharacter {
        SkinnedMeshRenderer SkinnedMeshRender { get; }

        // ReSharper disable once InconsistentNaming
        GameObject gameObject { get;  }
        ModelDefinition Model { get; }
        CharacterEvents Events { get; }

        BodyOptions RenderOptions { get; }
        DisplaySettings DisplaySettings { get; }
        MeshCorrection MeshCorrection { get; }
        Gender Gender { get; }

        IndividualizedBody Body {
            get;
        }
        
        void StartAnimation(MoshAnimation moshAnimation, PlaybackSettings playbackSettings, DisplaySettings characterSettings, BodyOptions renderOptions);
        void InterruptAnimation();

        void SetIndex(int animationIndex);

    }
}