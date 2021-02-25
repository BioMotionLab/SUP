using System.Collections;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace Utilities {
    public class DebugTools : MonoBehaviour
    {
        [PublicAPI]
        static void DebugArray(string name, IList list)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{name}: length {list.Count}");
            foreach (object item in list) sb.AppendLine(item.ToString());
            Debug.Log(sb);
        }
    }
}
