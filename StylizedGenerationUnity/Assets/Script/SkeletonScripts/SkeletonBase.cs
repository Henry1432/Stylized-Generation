using Palmmedia.ReportGenerator.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class SkeletonBase : MonoBehaviour
{
    //this should be the thing that tracks all the nodes and how I interact with each joint
    public static SkeletonBase instance;
    public List<SkeletonNode> allNodes = new List<SkeletonNode>();
    public SkeletonNode baseNode;
    public List<Joint> joints = new List<Joint>();
    public List<Vector3> angles = new List<Vector3>();
    public List<Vector3> baseNormals = new List<Vector3>();
    public List<Vector3> startNormals = new List<Vector3>();


    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        //should cut out non children in loop below instead of bailing
        allNodes.AddRange(FindObjectsOfType<SkeletonNode>());

        foreach (SkeletonNode node in allNodes)
        {
            if(node.parentIndex == -1)
            {
                baseNode = node;
                break;
            }
        }

        Joint[] thisJoints = baseNode.gameObject.GetComponents<Joint>();
        

        foreach (Joint joint in thisJoints)
        {
            SetParentJoint(joint);
        }
        startNormals.AddRange(baseNormals);
        SetGlobals(baseNode);
    }

    private void Update()
    {
        //handle local rotations based on joint angles here so that the rotation doesnt cascade down.
        //it was happening because when you used sourcenode rotation it would do that for all below doubling and doubling the rotation

        baseNode.localTrans = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        SetJoints();
        SetGlobals(baseNode);


    }

    public void SetJoints()
    {
        foreach(Joint joint in joints)
        {
            joint.rotations.Clear();
        }

        if(joints.Count == angles.Count && joints.Count > 0)
        {
            for(int i = 0; i < joints.Count; i++)
            {
                joints[i].normal = (Quaternion.Euler(angles[i]) * baseNormals[i]);
                Quaternion jointRotation = Quaternion.FromToRotation(baseNormals[i], joints[i].normal);
                joints[i].targetRotation = jointRotation;
                EditJointsNormalDown(joints[i], jointRotation);
            }
        }
    }
    private void EditJointsNormalDown(Joint j, Quaternion r)
    {
        int index = joints.IndexOf(j);
        baseNormals[index] = r * startNormals[index];
        foreach(Quaternion q in joints[index].rotations)
        {
            baseNormals[index] = q * baseNormals[index];
        }
        joints[index].rotations.Add(r);
        Joint[] tJoints = j.target.gameObject.GetComponents<Joint>();
        foreach (Joint joint in tJoints)
        {
            if(joints.Contains(joint))
            {
                EditJointsNormalDown(joint, r);
            }
        }
    }

    private void SetGlobals(SkeletonNode node)
    {
        node.globalTrans = node.SetGlobal();
        Joint[] thisJoints = node.gameObject.GetComponents<Joint>();

        foreach (Joint joint in thisJoints)
        {
            SetGlobals(joint.target);
        }
    }

    private void SetParentJoint(Joint joint)
    {
        if (!joints.Contains(joint))
        {
            joints.Add(joint);
            angles.Add(new Vector3());
            baseNormals.Add(joint.normal);
        }

        SkeletonNode sourceNode = joint.sourse.gameObject.GetComponent<SkeletonNode>();
        joint.target.parentIndex = allNodes.IndexOf(sourceNode);
        Joint testJoint = joint.target.gameObject.GetComponent<Joint>();

        if (testJoint != null)
        {
            SetParentJoint(testJoint);
        }
    }
}
