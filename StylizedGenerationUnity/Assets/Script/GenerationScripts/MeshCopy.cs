using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class MeshCopy : MonoBehaviour
{
    public SplineGenerator splineGen;
    Mesh mesh;
    public List<Vector3> verts;
    public List<Vector2> uvs;
    public List<int> triangles;

    public Material mat;
    public Transform first;

    int speed = 10;
    public float radius;
    const int SEGMENTS = 50;
    bool ran = false;
    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();

        splineGen = FindObjectOfType<SplineGenerator>();


        if (splineGen == null)
        {
            splineGen = FindObjectOfType<SplineGenerator>();
        }


    }

    // Update is called once per frame
    void Update()
    {
        if (!ran && splineGen.splines.Count != 0)
        {
            int counter = 0;
            GameObject obj = new GameObject("Mesh", typeof(MeshFilter), typeof(MeshRenderer));
            Vector3 up = first.up;
            
            foreach (Spline s in splineGen.splines)
            {
                foreach (Vector3 point in s.points)
                {
                    if (counter == speed)
                    {
                        up = Quaternion.FromToRotation(s.direction[(s.points.IndexOf(point) - 1)], s.direction[s.points.IndexOf(point)]) * up;
                        verts.AddRange(CreateVerts(point, s.direction[s.points.IndexOf(point)], up, 0.5f, SEGMENTS));
                        counter = 0;
                    }
                    else
                    {
                        counter++;
                    }

                }
            }
            mesh.vertices = verts.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.triangles = triangles.ToArray();
            obj.GetComponent<MeshFilter>().mesh = mesh;
            obj.GetComponent<MeshRenderer>().material = mat;
            ran = true;
        }

    }

    List<Vector3> CreateVerts(Vector3 center, Vector3 dir, Vector3 up, float radius, int segments)
    {
        List<Vector3> verts = new List<Vector3>();
        Vector3 tempUp = up;
        for(int i = 0; i < segments; i++)
        {
            verts.Add(center + (radius * tempUp));
            float theta = (360 / segments) * i;
            Vector3 pt = Quaternion.AngleAxis(theta, dir) * tempUp;
            tempUp = pt.normalized;
        }
        verts.RemoveAt(0);
        return verts;
    }
   
}