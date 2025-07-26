using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedTargetAxis : TargetAxis
{
    public Vector3 referenceVector;

    void SetAxis()
    {
        if(target == null)
        {
            target = transform;
        }
        forward = target.forward;
        up = target.up;
        forward = Quaternion.FromToRotation(up,referenceVector) * forward;
        forward = forward.normalized;
        right = Vector3.Cross(referenceVector,forward).normalized;
        up = referenceVector.normalized;
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
