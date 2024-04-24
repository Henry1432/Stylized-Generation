using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using static UnityEngine.Rendering.HableCurve;

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
                        verts.AddRange(CreateVerts(point, s.direction[s.points.IndexOf(point)], up, 1f, SEGMENTS));
                       
                        counter = 0;
                    }
                    else
                    {
                        counter++;
                    }

                }
            }
            triangles.AddRange(CreateIndices(verts));
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

    List<int> CreateIndices(List<Vector3> v)
    {
        List<int> indices = new List<int>();
        for(int i = 0; i < v.Count - SEGMENTS; i++)
        {
            // if final leg of segment - wrap around
            if((i + 1) % SEGMENTS == 0)
            {
                indices.Add(i); //final point current seg
                indices.Add(i + SEGMENTS); // final point on next seg
                indices.Add(i - SEGMENTS + 1);// first point current seg
            }
            else
            {
                indices.Add(i); // current point current seg
                indices.Add(i + SEGMENTS); // current point next seg
                indices.Add(i + 1); // next point current seg
            }
        }
        // for final segment, wrap around to first
        for(int j = v.Count - SEGMENTS; j < SEGMENTS; j++)
        {
            if ((j + 1) % SEGMENTS == 0)
            {
                indices.Add(j); //final point final seg
                indices.Add(0); // first point first seg
                indices.Add(j - SEGMENTS + 1);//first point final seg
            }
            else
            {
                indices.Add(j); //start point final seg
                indices.Add(j % SEGMENTS); //next point first seg
                indices.Add(j + 1); // next point final seg
            }
        }
        return indices;
    }
}

