using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplinePoint : MonoBehaviour
{
    public Transform prePoint;
    public Transform postPoint;

    private void Start()
    {
        Transform[] children = gameObject.GetComponentsInChildren<Transform>();
        if (children.Length >= 2)
        {
            prePoint = children[1];
            prePoint.GetComponent<subPointRender>().pre = true;
            postPoint = children[2];
            postPoint.GetComponent<subPointRender>().pre = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(transform.position, 0.125f);
    }
}
