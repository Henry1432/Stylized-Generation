using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinForFun : MonoBehaviour
{
    [SerializeField] private float mRotation;
    [Range(0.01f, 0.25f)]
    [SerializeField] private float mSpeed = 0.025f;

    void FixedUpdate()
    {
        transform.rotation = Quaternion.EulerAngles(transform.rotation.x, transform.rotation.y + mRotation, transform.rotation.z);
        if(mRotation < float.PositiveInfinity - 1000)
            mRotation += mSpeed;
    }
}
