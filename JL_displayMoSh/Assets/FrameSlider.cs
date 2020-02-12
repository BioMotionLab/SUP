using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FrameSlider : MonoBehaviour
{
    Slider slider;

    [SerializeField]
    TextMeshProUGUI FrameValue;
    
    
    // Start is called before the first frame update
    void OnEnable() {
        slider = GetComponent<Slider>();
        PlaybackEventSystem.OnFrameBroadcast += SetFrame;
        PlaybackEventSystem.OnBroadcastTotalFrames += SetTotalFrames;
    }

    void OnDisable() {
        PlaybackEventSystem.OnFrameBroadcast -= SetFrame;
        PlaybackEventSystem.OnBroadcastTotalFrames -= SetTotalFrames;
    }

    void SetTotalFrames(int totalFrames) {
        slider.maxValue = totalFrames;
        Debug.Log($"set max frames to {slider.maxValue}");
    }

    void SetFrame(float frame) {
        slider.SetValueWithoutNotify(frame);
        FrameValue.SetText(frame.ToString("F1"));
    }

    [PublicAPI]
    public void UserChangedFrame(float value) {
        PlaybackEventSystem.UserSelectedFrame(value);
    }

    [PublicAPI]
    public void UserChangedFrame(string value) {
        float floatValue = float.Parse(value);
        UserChangedFrame(floatValue);
        Debug.Log($"Typed {floatValue}");
    }
}
