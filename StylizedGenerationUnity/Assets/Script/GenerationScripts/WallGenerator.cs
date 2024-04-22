using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallGenerator : MonoBehaviour
{
    public GameObject wall;
    public SplineGenerator splineGen;
    // Start is called before the first frame update
    void Start()
    {
       splineGen = FindObjectOfType<SplineGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(splineGen == null)
        {
            splineGen = FindObjectOfType<SplineGenerator>();
        }
        // O(n^2)
        foreach(Spline s in splineGen.splines)
        {
            foreach(Vector3 point in s.points)
            {
                Instantiate(wall, point, s.transform.rotation);
            }
        }
    }
}
