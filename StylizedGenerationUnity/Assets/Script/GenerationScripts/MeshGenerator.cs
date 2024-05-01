using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using static UnityEngine.Rendering.HableCurve;
using System.Linq;
using System.Drawing;
using static UnityEngine.Rendering.DebugUI.Table;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using System.Reflection;

public class MeshCopy : MonoBehaviour
{
    public SplineGenerator splineGen;
    public List<Vector3> verts;
    public List<Vector2> uvs;
    public List<int> triangles;

    public Material mat;
    public Transform first;

    int speed = 10;
    int frames = 0;

    [Range(0.5f, 10)]
    public float radius;

    const int SEGMENTS = 100;
    bool ran = false;

    [Range(0.1f, 0.2f)]
    public float gNoise;

    // Start is called before the first frame update
    void Start()
    {
        splineGen = FindObjectOfType<SplineGenerator>(); 
    }

    // Update is called once per frame
    void Update()
    {
        if (splineGen == null)
        {
            splineGen = FindObjectOfType<SplineGenerator>();
        }
        if (!ran && splineGen.splines.Capacity != 0 && frames >= 2)
        {
            GenerateMesh();
        }
        frames++; // giving a two frame buffer allows closed splines to close

        if (Input.GetKeyDown(KeyCode.R) && ran)
        {
            Rerun();
        }
    }

    List<Vector3> CreateVerts(Vector3 center, Vector3 dir, Vector3 up, float radius, float noise)
    {
        List<Vector3> verts = new List<Vector3>();
        Vector3 f = dir.normalized;
        Vector3 r = Vector3.Cross(f, up).normalized;
        Vector3 u = Vector3.Cross(r, f).normalized;
        for (int i = 0; i <= SEGMENTS; i++)
        {
            float theta = (2 * Mathf.PI / SEGMENTS) * i;
            
            Vector3 pos = center + (r * Mathf.Cos(theta) + u * Mathf.Sin(theta) * radius);
            Vector3 toCenter = pos - center;
            Vector3 toAdd = pos + toCenter * UnityEngine.Random.Range(-noise, noise);
            verts.Add(toAdd);
        }
        
        return verts;
    }

    List<int> CreateIndices(List<Vector3> v)
    {
        List<int> indices = new List<int>();
        int points = 0;
        foreach (Spline s in splineGen.splines)
        {
            foreach (Vector3 p in s.points)
            {
                points++;
            }
        }
        int columns = SEGMENTS + 1;
        int rings = v.Count / SEGMENTS;
        Debug.Log(points); Debug.Log(rings);

        for (int row = 0; row < rings - 1; row++)
        {
            for (int col = 0; col < SEGMENTS; col++)
            {
                int start = row * columns + col;
                indices.Add(start);

                indices.Add(start + 1);

                indices.Add(start + columns);

                indices.Add(start + columns);

                indices.Add(start + 1);

                indices.Add(start + columns + 1);
            }
        }
        return indices;
    }
    List<Vector2> CreateUVs(List<Vector3> v)
    {
        List<Vector2> uvs = new List<Vector2>();
        for(int i = 0; i < v.Count; i++)
        {
            uvs.Add(new Vector2(v[i].x, v[i].z));
        }
        return uvs;
    }
    void GenerateMesh()
    {
        splineGen = FindObjectOfType<SplineGenerator>();
        Mesh mesh = new Mesh();
        GameObject obj = new GameObject("Mesh", typeof(MeshFilter), typeof(MeshRenderer));
        Vector3 up = first.up;
        int counter = 0;
        foreach (Spline s in splineGen.splines)
        {
            foreach (Vector3 point in s.points)
            {
                if (counter == speed)
                {
                    up = Vector3.up;
                    up = Quaternion.FromToRotation(s.direction[s.points.IndexOf(point) - 1], s.direction[s.points.IndexOf(point)]) * up;
                    verts.AddRange(CreateVerts(point, s.direction[s.points.IndexOf(point)], up, 2f, gNoise));

                    counter = 0;
                }
                else
                {
                    counter++;
                }

            }
        }
        uvs.AddRange(CreateUVs(verts));
        triangles.AddRange(CreateIndices(verts));
        mesh.vertices = verts.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();
        obj.GetComponent<MeshFilter>().mesh = mesh;
        obj.GetComponent<MeshRenderer>().material = mat;
        ran = true;
    }

    public void GenerateNoise()
    {

    }

    [ExecuteInEditMode]
    public void Rerun()
    {
        MeshRenderer obj = FindObjectOfType<MeshRenderer>();
        Destroy(obj);
        verts.Clear();
        uvs.Clear();
        triangles.Clear();
        GenerateMesh();
    }
}