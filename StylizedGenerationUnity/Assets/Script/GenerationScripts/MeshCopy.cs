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

public class Vert
{
    Vector3 pos;
    int index;
}
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
    int frames = 0;
    public float radius;
    const int SEGMENTS = 8;
    bool ran = false;
    bool quads = false;
    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();

        splineGen = FindObjectOfType<SplineGenerator>();


        if (splineGen == null)
        {
            splineGen = FindObjectOfType<SplineGenerator>();
        }
        quads = false;
        //quads = true;

    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space));
        //{
        //    if(quads)
        //    {
        //        quads = false;
        //    }
        //    else
        //    {
        //        quads = true;
        //    }
                
        //}
        
        if (!ran && splineGen.splines.Capacity != 0 && frames >= 2)
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
                        verts.AddRange(CreateVerts(point, s.direction[s.points.IndexOf(point)], up, 2f));
                       
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
            //mesh.SetIndices(triangles.ToArray(), MeshTopology.Lines, 0);
            ran = true;
        }
        frames++; // giving a two frame buffer allows closed splines to close
    }

    List<Vector3> CreateVerts(Vector3 center, Vector3 dir, Vector3 up, float radius)
    {
        List<Vector3> verts = new List<Vector3>();
        Vector3 tempUp = up;
        for (int i = 0; i <= SEGMENTS; i++)
        {
            verts.Add(center + (radius * tempUp));
            float theta = (360 / SEGMENTS) * i;
            Vector3 pt = Quaternion.AngleAxis(theta, dir) * tempUp;
            tempUp = pt.normalized;
        }

      //Vector3 first = verts.First();
      //verts.RemoveAt(0);
      //verts.Add(first);

        return verts;
    }

    List<int> CreateIndices(List<Vector3> v)
    {
        List<int> indices = new List<int>();
        int points = 0;
        foreach(Spline s in splineGen.splines)
        {
            foreach(Vector3 p in s.points)
            {
                points++;
            }
        }
        int columns = SEGMENTS + 1;
        int rings = v.Count / SEGMENTS;
        Debug.Log(points); Debug.Log(rings);
        for (int row = 0; row < 1; row++)
        {
            for (int col = 0; col < SEGMENTS; col++)
            {
                 int start = row * columns + col;
                 indices.Add(start);
            
                 indices.Add(start + 1);
            
                 indices.Add(start + columns);
            }
        }
        return indices;
    }
}
//for (int i = 0; i < v.Count; i++) // real for loop
//for(int i = 0; i < SEGMENTS * 5; i++) // debug for loop
//    //if final point of segment - wrap around to first point
//    if ((i + 1) % SEGMENTS == 0)
//    {

//        indices.Add(i + SEGMENTS);     // current point next seg
//        indices.Add(i);                // current point current seg
//        indices.Add(i - SEGMENTS + 1); // first point current seg
//        if (quads)
//        {
//            indices.Add(i + SEGMENTS + 1); // first point next seg
//            indices.Add(i + SEGMENTS);     // current point next seg
//            indices.Add(i - SEGMENTS + 1); // first point current seg
//        }

//    }
//    else
//    {
//        indices.Add(i + SEGMENTS);      // current point next seg
//        indices.Add(i);                 // current point current seg
//        indices.Add(i + 1);             // next point current seg
//        if (quads)                               
//        {
//           indices.Add(i + SEGMENTS + 1);  // next point next seg
//           indices.Add(i + SEGMENTS);      // current point next seg
//           indices.Add(i + 1);             // point after current seg
//        }


//    }
//}
//// for final segment, wrap around to first segment
////for (int j = v.Count - SEGMENTS; j < v.Count; j++)
////{
////    // if final point of segment - wrap around to first point
////    if ((j + 1) % SEGMENTS == 0)
////    {
////        indices.Add(0);                // first point first seg
////        indices.Add(j);                // final point final seg
////        indices.Add(j - SEGMENTS + 1); // first point final seg
////        if(quads)
////        {
////           indices.Add(1);                // next  point first seg
////           indices.Add(0);                // first point first seg
////           indices.Add(j - SEGMENTS + 1); // first point final seg
////        }


////    }
////    else
////    {
////        indices.Add(j % SEGMENTS);     // current point first seg 
////        indices.Add(j);                // current point final seg   
////        indices.Add(j + 1);            // next point final seg
////        if(quads)
////        {
////            indices.Add(j + 1 % SEGMENTS); // next point first seg
////            indices.Add(j % SEGMENTS);     // current point first seg
////            indices.Add(j + 1);            // next point final seg          
////        }
////    }
////}