using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetAxis : MonoBehaviour
{
    public Transform target;
    [System.NonSerialized] public Vector3
        forward,up,right;
    public bool runOnFixedUpdate;

    void SetAxis()
    {
        if(target == null)
        {
            target = transform;
        }
        forward = target.forward;
        up = target.up;
        right = target.right;
    }

    // Update is called once per frame
    void Update()
    {
        if(!runOnFixedUpdate)
        {
            SetAxis();
        }
    }

    void FixedUpdate()
    {
        if(runOnFixedUpdate)
        {
            SetAxis();
        }
    }
}
