using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
// ReSharper disable All

namespace MoshPlayer.ThirdParty.StandaloneFileBrowser.Sample {
    [RequireComponent(typeof(Button))]
    public class CanvasSampleOpenFileText : MonoBehaviour, IPointerDownHandler {
        public Text output;

#if UNITY_WEBGL && !UNITY_EDITOR
    //
    // WebGL
    //
    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);

    public void OnPointerDown(PointerEventData eventData) {
        UploadFile(gameObject.name, "OnFileUpload", ".txt", false);
    }

    // Called from browser
    public void OnFileUpload(string url) {
        StartCoroutine(OutputRoutine(url));
    }
#else
        //
        // Standalone platforms & editor
        //
        public void OnPointerDown(PointerEventData eventData) { }

        void Start() {
            var button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        private void OnClick() {
            var paths = global::ThirdParty.StandaloneFileBrowser.StandaloneFileBrowser.OpenFilePanel("Title", "", "txt", false);
            if (paths.Length > 0) {
                //StartCoroutine(OutputRoutine(new System.Uri(paths[0]).AbsoluteUri));
            }
        }
#endif

    }
}