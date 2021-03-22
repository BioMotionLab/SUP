using Playback;
using UnityEngine;

namespace InGameUI {
    public class UIManager : MonoBehaviour
    {
    
        [SerializeField] GameObject UI = default;
        [SerializeField] KeyCode toggleKey = KeyCode.U;
        void OnEnable()
        {
            if (UI != null) UI.SetActive(true);
            Debug.Log(Format.Log($"To toggle UI, press the {toggleKey} key"));
        }

        // Update is called once per frame
        void Update() {
            if (Input.GetKeyDown(toggleKey)) {
                UI.SetActive(!UI.activeSelf);
            }
        }
    }
}
