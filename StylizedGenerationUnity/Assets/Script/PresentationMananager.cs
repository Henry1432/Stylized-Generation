using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PresentationMananager : MonoBehaviour
{
    public ShowFilterV1 v1Target;
    public ShowFilterV2 v2Target;
    public bool final;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (final)
                v2Target.enabled = !v2Target.enabled;
            else
            {
                v1Target.enabled = !v1Target.enabled;
                v2Target.enabled = !v2Target.enabled;
            }
        }

        if(Input.GetKeyDown(KeyCode.V) && !final) {
            v1Target.kernelSize = 14;
        }
        
    }
}
