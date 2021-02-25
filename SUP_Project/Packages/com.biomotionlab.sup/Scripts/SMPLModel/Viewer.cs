using Display;
using Playback;
using Settings;

namespace SMPLModel {
    public interface Viewer {
        BodyOptions RuntimeBodyOptions { get; }
        DisplaySettings RuntimeDisplaySettings { get; }
        PlaybackSettings RuntimePlaybackSettings { get; }
    }
}