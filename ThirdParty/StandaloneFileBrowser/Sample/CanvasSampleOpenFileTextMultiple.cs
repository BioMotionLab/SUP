using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ThirdParty.StandaloneFileBrowser.Sample {
    [RequireComponent(typeof(Button))]
    public class CanvasSampleOpenFileTextMultiple : MonoBehaviour, IPointerDownHandler {
        public Text output;

#if UNITY_WEBGL && !UNITY_EDITOR
    //
    // WebGL
    //
    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);

    public void OnPointerDown(PointerEventData eventData) {
        UploadFile(gameObject.name, "OnFileUpload", ".txt", true);
    }

    // Called from browser
    public void OnFileUpload(string urls) {
        StartCoroutine(OutputRoutine(urls.Split(',')));
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
            // var paths = StandaloneFileBrowser.OpenFilePanel("Title", "", "txt", true);
            var paths = global::ThirdParty.StandaloneFileBrowser.StandaloneFileBrowser.OpenFilePanel("Open File", "", "", true);
            if (paths.Length > 0) {
                var urlArr = new List<string>(paths.Length);
                for (int i = 0; i < paths.Length; i++) {
                    urlArr.Add(new System.Uri(paths[i]).AbsoluteUri);
                }
                //StartCoroutine(OutputRoutine(urlArr.ToArray()));
            }
        }
#endif


    }
}