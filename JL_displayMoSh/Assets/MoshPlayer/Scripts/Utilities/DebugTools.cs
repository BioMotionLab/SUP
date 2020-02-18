using UnityEngine;


public class DebugTools : MonoBehaviour
{
    SkinnedMeshRenderer skinnedMeshRenderer;

    // Start is called before the first frame update
    void Awake() {
        
    }

    [ContextMenu("Get Tip Of Finger")]
    void GetTipOfFinger() {
        
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        
        //get tip of right middle finger
        var max = Vector3.zero;
        var newMesh = new Mesh();
        skinnedMeshRenderer.BakeMesh(newMesh);
            
        foreach (var vertex in newMesh.vertices)
        {
            if (vertex.x > max.x) {
                max = vertex;
            }
                
        }
            
        Debug.Log($"max: {max.ToString("F6")}");
        Vector3 pelv = skinnedMeshRenderer.bones[0].position;
        Vector3 local = max - pelv;
        Debug.Log($"pelv world: {pelv} max local: {local.ToString("F6")}");

    }
}
