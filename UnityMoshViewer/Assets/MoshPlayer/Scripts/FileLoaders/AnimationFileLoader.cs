using MoshPlayer.Scripts.Playback;

namespace MoshPlayer.Scripts.FileLoaders {
    public interface AnimationFileLoader {
        MoshAnimation BuildWithSettings();
    }
}