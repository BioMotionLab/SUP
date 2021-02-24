using System;
using JetBrains.Annotations;
using Playback;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InGameUI {
    public class FrameSlider : MonoBehaviour
    {
        Slider slider;
    
        [SerializeField]
        TextMeshProUGUI FrameValue = default;

        AnimationControlEvents animationControlEvents;


        // Start is called before the first frame update
        void OnEnable() {
            slider = GetComponent<Slider>();
        }

        public void Init(AnimationControlEvents animationControlEventsToAttach) {
            animationControlEvents = animationControlEventsToAttach ?? throw new NullReferenceException("Animation events null");
            animationControlEvents.OnFrameBroadcast += SetFrame;
        }

        void OnDisable() {
            if (animationControlEvents == null) return;
            animationControlEvents.OnFrameBroadcast -= SetFrame;
        }
        
        

        void SetFrame(FrameData frameData) {
            slider.maxValue = frameData.TotalFrames;
            slider.SetValueWithoutNotify(frameData.CurrentFrame);

            float duration = frameData.TotalFrames / (float)frameData.FrameRate;
            float currentTime = frameData.CurrentFrame / frameData.FrameRate;

            FrameValue.SetText($"Frame: {frameData.CurrentFrame:F0}/{frameData.TotalFrames}. Time (s): {currentTime:F1}/{duration:F1}");
        }

        [PublicAPI]
        public void UserChangedFrame(float value) {
            animationControlEvents?.UserSelectedFrame(value);
        }

        [PublicAPI]
        public void UserChangedFrame(string value) {
            try {
                float floatValue = float.Parse(value);
                UserChangedFrame(floatValue);
                Debug.Log($"Typed {floatValue}");
            }
            catch (FormatException) { }
        }
    }
}
