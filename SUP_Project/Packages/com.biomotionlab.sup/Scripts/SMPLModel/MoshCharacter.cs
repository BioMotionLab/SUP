using Display;
using MoshPlayer;
using Playback;
using UnityEngine;

namespace SMPLModel {
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