using System;
using JetBrains.Annotations;
using Playback;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace InGameUI {
    public class FrameSlider : MonoBehaviour
    {
        Slider slider;
    
        [FormerlySerializedAs("FrameValue")] [SerializeField]
        TextMeshProUGUI frameValue = default;

        AnimationControlEvents animationControlEvents;


        // Start is called before the first frame update
        void OnEnable() {
            slider = GetComponent<Slider>();
        }

        public void Init(AnimationControlEvents animationControlEventsToAttach) {
            animationControlEvents = animationControlEventsToAttach ?? throw new NullReferenceException("Animation events null");
            animationControlEvents.OnFrameBroadcast += SetFrame;
            animationControlEvents.OnBroadcastTotalFrames += SetTotalFrames;
        }

        void OnDisable() {
            if (animationControlEvents == null) return;
            animationControlEvents.OnFrameBroadcast -= SetFrame;
            animationControlEvents.OnBroadcastTotalFrames -= SetTotalFrames;
        }

        void SetTotalFrames(int totalFrames) {
            slider.maxValue = totalFrames;
            //Debug.Log($"set max frames to {slider.maxValue}");
        }

        void SetFrame(float frame) {
            slider.SetValueWithoutNotify(frame);
            frameValue.SetText(frame.ToString("F1"));
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
