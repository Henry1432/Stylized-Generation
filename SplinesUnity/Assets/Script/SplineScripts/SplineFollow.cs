using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;

public class SplineFollow : MonoBehaviour
{
    public SplineGenerator fullSpline;
    [SerializeField] private Spline currentSpline;
    private int index;
    public bool continuous;
    public int speed = 5;

    private void FixedUpdate()
    {
        if(currentSpline)
        {
            if(index < currentSpline.points.Count)
            {
                transform.forward = currentSpline.direction[index];
                transform.position = currentSpline.points[index];
                index+= speed;
            }
            else
            {
                bool checkLast;
                currentSpline = fullSpline.getNextSpline(gameObject.name, continuous, out checkLast);
                if(!(currentSpline == fullSpline.splines.Last() && checkLast))
                {
                    index = 0;
                }
            }
        }
        else
        {
            if (fullSpline.splines.Count > 0)
                currentSpline = fullSpline.getNextSpline(gameObject.name, continuous);
        }
    }

}
