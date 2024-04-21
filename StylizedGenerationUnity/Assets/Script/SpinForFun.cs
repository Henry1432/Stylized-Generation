using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinForFun : MonoBehaviour
{
    [SerializeField] private float mSpeed;

    void FixedUpdate()
    {
        transform.rotation = Quaternion.EulerAngles(transform.rotation.x, transform.rotation.y + mSpeed, transform.rotation.z);
        if(mSpeed < float.PositiveInfinity - 1000)
            mSpeed += Time.deltaTime;
    }
}
