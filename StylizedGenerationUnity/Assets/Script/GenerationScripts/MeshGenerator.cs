using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public Vector3 minBounds;
    public Vector3 maxBounds;
    public int numPoints;
    public float interval;
    public SplinePoint point;
    public SplineGenerator gen;
    public List<SplinePoint> points;
    // Start is called before the first frame update
    void Start()
    {
        gen = GetComponentInChildren<SplineGenerator>();
        for (int i = 0; i < numPoints; i++)
        {
            Vector3 randPos = new Vector3(Random.Range(minBounds.x, maxBounds.x), Random.Range(minBounds.y, maxBounds.y), Random.Range(minBounds.z, maxBounds.z));
            Vector3 randRot = new Vector3(Random.Range(0, 180), Random.Range(0, 180), Random.Range(0, 180));
            Quaternion randQ = new Quaternion(randRot.x, randRot.y, randRot.z, 0);
            Instantiate(point, randPos, randQ);
            point.prePoint = point.transform.GetChild(0);
            point.postPoint = point.transform.GetChild(1);
            points.Add(point);
        }
        gen.points = points;
        for (int i = 0; i < numPoints - 1; i++)
        {
            gen.CreateSpline(points[i], points[i + 1]);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
