using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enigma;

public class SimpleWire : MonoBehaviour
{
    public PhysicsEntity entity;
    public LineRenderer leftWire;
    public LineRenderer rightWire;
    public Transform leftHand;
    public Transform rightHand;
    public float veloInfluence;
    public float dampTime;
    public float wireLength;
    public Vector3 targetPos;
    Vector3 lVelo,rVelo;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        leftWire.SetPosition(0,leftHand.position);
        rightWire.SetPosition(0,rightHand.position);
        Vector3 velo = entity.body.velocity * veloInfluence;

        Vector3 lPos = leftHand.position-Vector3.up*wireLength - velo;
        Vector3 lDamped = Vector3.SmoothDamp(leftWire.GetPosition(1),lPos,ref lVelo,dampTime,50,Time.fixedDeltaTime);
        leftWire.SetPosition(1, lDamped);

        if(targetPos == Vector3.zero)
        {
            Vector3 rPos = rightHand.position-Vector3.up*wireLength - velo;
            Vector3 rDamped = Vector3.SmoothDamp(rightWire.GetPosition(1),rPos,ref rVelo,dampTime,50,Time.fixedDeltaTime);
            rightWire.SetPosition(1, rDamped);
        }
        else
        {
            rightWire.SetPosition(1,targetPos);
        }
    }
}
