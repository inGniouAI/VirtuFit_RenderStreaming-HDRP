using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.RenderStreaming;
//using Unity.RenderStreaming.Samples;
using Unity.RenderStreaming.Signaling;

public class VirtufitRenderStreaming : MonoBehaviour
{
   // [SerializeField] Dropdown dropdownCamera;
   //     [SerializeField] Transform[] cameras;
        [SerializeField] VirtuFitCopyTransform copyTransform;
        [SerializeField] RenderStreaming renderStreaming;
    // Start is called before the first frame update
    void Start()
    {
 //dropdownCamera.onValueChanged.AddListener(OnChangeCamera);

            if (!renderStreaming.runOnAwake)
            {
                renderStreaming.Run();
            }        
    }
    void Update()
    {
        
    }
}
