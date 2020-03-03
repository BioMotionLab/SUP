using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace MoshPlayer.Scripts.InGameUI {
    public class ValueFromSliderDisplay : MonoBehaviour
    {
        TextMeshProUGUI textComponent;

        // Start is called before the first frame update
        void OnEnable() {
            textComponent = GetComponent<TextMeshProUGUI>();
        }

        [PublicAPI]
        public void UpdateValue(float value) {
            float percentage = Mathf.RoundToInt(value * 100);
            textComponent.text = $"{percentage} %";
        }
    
    }
}
