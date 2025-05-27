using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enigma;

public class SwingAction : Action
{
    PhysicsEntity entity;
    public Transform swingTarget;
    [Header("Inputs")]
    public ActionInput swing;
    [Header("Variables")]
    public float targetRange = 20;
    public float targetAngle = 70;
    public Vector2 swingSpeed;

    // Start is called before the first frame update
    void Start()
    {
        entity = GetComponentInChildren<PhysicsEntity>();
    }

    // Update is called once per frame
    void Update()
    {
        if(entity.grounded == false)
        {
            if(swing.GetInputDown()!=0 && entity.actionLock == false)
            {
                swingTarget = SwingCheck(targetRange,targetAngle);
                if(swingTarget != null)
                {
                    entity.ChangeState("Swing");
                    entity.actionLock = true;
                    entity.body.useGravity = false;
                    entity.body.velocity = Vector3.zero;

                }
            }
        }
    }

    Transform SwingCheck(float range, float maxAngle)
    {
        Collider[] colliderList = Physics.OverlapSphere(entity.body.position, range);
        Transform target = null; float minAngle = maxAngle;
        foreach(var hitCollider in colliderList)
        {
            if(hitCollider.tag == "Swing")
            {
                Vector3 hitPoint = hitCollider.ClosestPoint(transform.position);
                Vector3 hitDir = (hitPoint - transform.position).normalized;
                float hitAngle = Vector3.Angle(transform.forward, hitDir);
                if(hitAngle < minAngle)
                {
                    minAngle = hitAngle;
                    target = hitCollider.transform;
                }
            }
        }

        if(target != null)
        {
            return target;
        }

        return null;
    }
}
