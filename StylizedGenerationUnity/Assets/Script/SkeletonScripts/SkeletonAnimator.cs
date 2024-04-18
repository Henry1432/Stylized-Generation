using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class SkeletonAnimator : MonoBehaviour
{
    public SaveSkeletonInfo SaveSkeletonInfo = new SaveSkeletonInfo();

    public List<float> timeStamps = new List<float>();
    public float timer = 0;
    public int current;

    public bool loop;

    public List<Vector3> currentAngles = new List<Vector3>();
    float T;

    public void Start()
    {
        current = 0;
    }

    private void Update()
    {
        if (timer < timeStamps.Last())
        {
            timer += Time.deltaTime;

            if(timer > timeStamps[current])
            {
                current++;
            }
        }
        else if(loop)
        {
            timer = 0;
            current = 0;
        }
        else
        {
            timer -= Time.deltaTime;
            current = timeStamps.Count - 1;
        }

        SetSkeleton();
    }
    private void SetSkeleton()
    {
        if (current < timeStamps.Count)
        {
            currentAngles = SaveSkeletonInfo.angles;

            float lastStamp = timeStamps[current > 0 ? current - 1 : 0], thisStamp = timeStamps[current];
            if(thisStamp !=  lastStamp) 
            { 
                T = (timer - lastStamp) / (thisStamp - lastStamp);

                currentAngles = lerpAngles(lastStamp, thisStamp);
            }
        }
        //SaveSkeletonInfo.SetAnglesFromStamp(timeStamps[current]);
        //currentAngles = SaveSkeletonInfo.angles;

        SkeletonBase.instance.angles = currentAngles;
    }

    private List<Vector3> lerpAngles(float lastStamp, float thisStamp)
    {
        List<Vector3> returnAngles = new List<Vector3>();
        SaveSkeletonInfo.SetAnglesFromStamp(lastStamp);
        List<Vector3> lastAngles = new List<Vector3>(SaveSkeletonInfo.angles);
        SaveSkeletonInfo.SetAnglesFromStamp(thisStamp);
        List<Vector3> thisAngles = new List<Vector3>(SaveSkeletonInfo.angles);

        for (int i = 0; i < currentAngles.Count; i++)
        {
            Vector3 lAngle = Vector3.Lerp(lastAngles[i], thisAngles[i], T);
            Debug.Log(lAngle);
            returnAngles.Add(lAngle);
        }

        return returnAngles;
    }
}
