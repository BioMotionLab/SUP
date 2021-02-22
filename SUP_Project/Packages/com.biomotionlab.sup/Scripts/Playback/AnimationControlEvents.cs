namespace MoshPlayer.Scripts.Playback {
    public class AnimationControlEvents {
    
        public delegate void FrameBroadCastEvent(float frame);

        public event FrameBroadCastEvent OnFrameBroadcast;

        public void BroadcastCurrentFrame(float frame) {
            OnFrameBroadcast?.Invoke(frame);
        }
    
        public delegate void BroadcastTotalFramesEvent(int totalFrames);

        public event BroadcastTotalFramesEvent OnBroadcastTotalFrames;
        public void BroadcastTotalFrames(int totalFrames) {
            OnBroadcastTotalFrames?.Invoke(totalFrames);
        }
    
        public delegate void UserFrameSelectEvent(float frame);

        public event UserFrameSelectEvent OnUserFrameSelect;

        public void UserSelectedFrame(float frame) {
            OnUserFrameSelect?.Invoke(frame);
        }
    
        public delegate void AnimationEndedEvent();

        public event AnimationEndedEvent OnAnimationEnded;

        public void BroadCastAnimationEnded() {
            OnAnimationEnded?.Invoke();
        }

        public delegate void AnimationStartedEvent(MoshAnimation moshAnimation, AnimationControlEvents animationControlEvents);
     
        public static event AnimationStartedEvent OnAnimationStarted;
 
        public static void BroadCastAnimationStarted(MoshAnimation moshAnimation, AnimationControlEvents animationControlEvents) {
            OnAnimationStarted?.Invoke(moshAnimation, animationControlEvents);
        }
     
  

    }
}