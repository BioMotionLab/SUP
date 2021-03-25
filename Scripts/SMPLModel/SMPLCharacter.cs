using System.Windows.Forms;
using Display;
using Playback;
using Settings;
using UnityEngine;

namespace SMPLModel {
    public interface SMPLCharacter {
        SkinnedMeshRenderer SkinnedMeshRender { get; }

        // ReSharper disable once InconsistentNaming
        GameObject gameObject { get;  }
        ModelDefinition Model { get; }
        CharacterEvents Events { get; }

        BodySettings RenderSettings { get; }
        DisplaySettings DisplaySettings { get; }
        MeshCorrection MeshCorrection { get; }
        Gender Gender { get; }

        IndividualizedBody Body {
            get;
        }
        
        void StartAnimation(AMASSAnimation amassAnimation, PlaybackSettings playbackSettings, DisplaySettings characterSettings, BodySettings renderSettings);
        void InterruptAnimation();

        void SetIndex(int animationIndex);

        void SetOrigin(Transform testOrigin);
    }
}