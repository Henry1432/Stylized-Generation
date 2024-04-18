using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Spline : MonoBehaviour
{
    public SplineGenerator splineGenerator;
    public List<Vector3> points = new List<Vector3>();
    public List<Vector3> direction = new List<Vector3>();
    public int editIndex = 0;

    public void DeleteSpline()
    {
        StartCoroutine(DestroyEndOfFrame());
    }

    IEnumerator DestroyEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        splineGenerator.splines.Remove(this);
        Destroy(this.gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLineList(points.ToArray());
    }

    public void AddPoint(Vector3 point, Vector3 dir)
    {
        if (editIndex >= points.Count - 1)
        {
            if (points.Count > 0)
            {
                points.Add(point);
                direction.Add(dir);
                editIndex++;
            }
            points.Add(point);
            direction.Add(dir);
            editIndex++;
        }
        else
        {
            points[editIndex] = point;
            direction[editIndex] = dir;
            editIndex++;
            if(editIndex > 1)
            {
                points[editIndex] = point;
                direction[editIndex] = dir;
                editIndex++;
            }

        }
    }
    public void FixLists()
    {
        points.Remove(points.Last());
        direction.Remove(direction.Last());
    }

    ~Spline()
    {
        if(splineGenerator.splines.Contains(this))
        {
        }
    }
}
