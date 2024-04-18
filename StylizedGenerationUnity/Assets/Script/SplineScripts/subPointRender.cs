using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class subPointRender : MonoBehaviour
{
    public bool pre;
    private void OnDrawGizmos()
    {
        if(pre)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.yellow;
        }
        Gizmos.DrawSphere(transform.position, 0.0625f);
    }
}
