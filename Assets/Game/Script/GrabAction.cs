using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enigma;

public class GrabAction : Action
{
    PhysicsEntity entity;
    Animator animator;
    public GameObject grabObject;
    public ShapeTrailRenderer shapeTrail;
    
    [Header("Inputs")]
    public ActionInput grab;
    public bool hold;

    [Header("Variables")]
    public float grabRange;
    public float grabAngle;
    public float grabStrength;
    public float grabLength;
    public float throwStrength;
    float holdTime;
    Vector3 holdDir;
    float releaseTime = 0;

    void Start()
    {
        entity = GetComponentInChildren<PhysicsEntity>();
        animator = transform.root.GetComponentInChildren<Animator>();
    }
    
    void Update()
    {
        if(hold == true && grab.GetInputUp()!=0)
        {
            hold = false;
        }   
        if(entity.grounded == true)
        {
            //Release
            if(grab.GetInput()!=0 && grabObject != null && hold == false)
            {
                if(releaseTime == 0)
                {
                    releaseTime = Time.time;
                }
                holdTime = Time.time-releaseTime;
                if(holdTime>0.5f)
                {
                    if(entity.ChangeState("Throwing"))
                    {
                        entity.actionLock = true;
                        holdDir = transform.forward;
                    }
                    entity.body.velocity = Vector3.zero;
                    animator.SetFloat("ActionFloat",Mathf.Clamp(holdTime,0,3));
                    ThrowingBehavior(grabObject);
                }
            }
            if(grabObject != null && holdTime != 0 && grab.GetInput() == 0)
            {
                if(holdTime>0.5f)
                {
                    entity.actionLock = false;
                    transform.forward = holdDir;
                    Rigidbody objBody = grabObject.GetComponent<Rigidbody>();
                    objBody.isKinematic = false;
                    float throwForce = Mathf.Clamp(holdTime,0,3)*Mathf.Clamp(holdTime,0,3)*throwStrength;
                    Vector3 tangent = Vector3.Cross(holdDir,transform.up);
                    objBody.velocity = (holdDir+tangent).normalized * throwForce + Vector3.up*0.1f;
                    objBody.angularVelocity = -Vector3.up * objBody.velocity.magnitude;
                    grabObject = null;
                    shapeTrail.endAttachment = null;
                    releaseTime = 0;
                    holdTime = 0;
                }
                else
                {
                    grabObject = null;
                    shapeTrail.endAttachment = null;
                    releaseTime = 0;
                    holdTime = 0;
                }

            }
            //Grabbing
            if(grab.GetInputDown()!=0 && entity.actionLock == false && grabObject == null)
            {
                grabObject = GrabCheck(grabRange,grabAngle);
                if(shapeTrail != null)
                {
                    shapeTrail.endAttachment = grabObject.transform;
                }
                hold = true;
                return;
            }
        }
    }

    void FixedUpdate()
    {
        if(grabObject != null)
        {
            ObjectBehavior(grabObject);
        }
    }

    void ObjectBehavior(GameObject grabObj)
    {
        Rigidbody objBody = grabObj.GetComponentInChildren<Rigidbody>();
        if(Vector3.Distance(entity.body.position,objBody.position) > grabLength)
        {
            Vector3 dir = (entity.body.position - objBody.position);
            objBody.velocity += dir*grabStrength*Time.fixedDeltaTime;
        }
    }

    void ThrowingBehavior(GameObject grabObj)
    {
        Rigidbody objBody = grabObj.GetComponentInChildren<Rigidbody>();
        objBody.isKinematic = true;
        Vector3 targetPos = transform.position+holdDir*grabLength/2;
        float yPos = Mathf.Lerp(objBody.position.y,targetPos.y + .25f,holdTime/3f);
        targetPos.y = yPos;
        objBody.position = Vector3.Lerp(objBody.position,targetPos,.1f*holdTime*holdTime);
        Quaternion objRot = Quaternion.LookRotation(Vector3.up,holdDir);
        objBody.rotation = Quaternion.Slerp(objBody.rotation,objRot,.1f*holdTime*holdTime);
        float angle = -(180*Time.deltaTime)*Mathf.Clamp(holdTime,0,3);
        holdDir = Quaternion.AngleAxis(angle,transform.up) * holdDir;
        Debug.DrawRay(transform.position,holdDir,Color.yellow);
        //objBody.position = Vector3.Lerp();
    }

    GameObject GrabCheck(float range, float maxAngle)
    {
        Collider[] colliderList = Physics.OverlapSphere(entity.body.position, range);
        Collider target = null; float minAngle = maxAngle;
        foreach(var hitCollider in colliderList)
        {
            if(hitCollider.tag == "Grab")
            {
                Vector3 hitPoint = hitCollider.ClosestPoint(transform.position);
                Vector3 hitDir = (hitPoint - transform.position).normalized;
                float hitAngle = Vector3.Angle(transform.forward, hitDir);
                if(hitAngle < minAngle)
                {
                    minAngle = hitAngle;
                    target = hitCollider;
                }
            }
        }

        if(target != null)
        {
            return target.gameObject;
        }

        return null;
    }
}
