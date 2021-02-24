using UnityEngine;

namespace InGameUI {
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(FlyCamera))]
    public class CameraControlsToggler : MonoBehaviour {
        FlyCamera flyCam;

        void Awake() {
            flyCam = GetComponent<FlyCamera>();
        }

        void OnEnable() {
            KeyboardControlEvents.OnDisableKeyboardControls += DisableFlyCam;
            KeyboardControlEvents.OnEnableKeyboardControls += EnableFlyCam;
        }
    
        void OnDisable() {
            KeyboardControlEvents.OnDisableKeyboardControls -= DisableFlyCam;
            KeyboardControlEvents.OnEnableKeyboardControls -= EnableFlyCam;
        }

        void EnableFlyCam() {
            flyCam.enabled = true;
        }

        void DisableFlyCam() {
            flyCam.enabled = false;
        }
    }
}
