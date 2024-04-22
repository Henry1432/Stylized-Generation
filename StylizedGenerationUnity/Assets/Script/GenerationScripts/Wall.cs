using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public SplineGenerator splineGen;
    public bool isOnSpline;
    // Start is called before the first frame update
    void Start()
    {
        splineGen = FindObjectOfType<SplineGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
        isOnSpline = false;
        foreach (Spline s in splineGen.splines)
        {
            foreach (Vector3 point in s.points)
            {
               if(point == transform.position)
                {
                    isOnSpline = true;
                    break;
                }
            }
        }
        if (!isOnSpline)
            Destroy(gameObject);
    }
}
