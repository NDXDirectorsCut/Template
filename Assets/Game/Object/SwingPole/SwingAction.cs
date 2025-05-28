using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enigma;

public class SwingAction : Action
{
    PhysicsEntity entity;
    public Collider swingTarget;
    [Header("Inputs")]
    public ActionInput swing;
    public ActionInput horizontal;
    public ActionInput vertical;

    [Header("Variables")]
    public float targetRange = 20;
    public float targetAngle = 70;
    public Vector2 swingSpeed;
    [Space(10)]
    public TargetAxis moveAxis;

    public float lerp;

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
                    StartCoroutine(Swing(swingTarget));
                }
            }
        }
    }

    IEnumerator Swing(Collider poleCollider)
    {
        Vector3 swingPoint = swingTarget.ClosestPoint(transform.position);
        Transform pole = poleCollider.transform.root;

        Vector3 upVector = (swingPoint - transform.position).normalized;
        Vector3 fwdVector = Vector3.Cross(upVector,pole.right);
        
        bool backwards = Vector3.Dot(transform.forward,fwdVector)<0 ? false : true;

        float fwdSpeed = 0;
        float rgtSpeed = 0;
        float fwdRef = 0;

        //Vector3 relVelocity = pole.InverseTransformDirection(Vector3.ProjectOnPlane(entity.body.velocity,upVector));
        fwdSpeed = 6 * Vector3.Dot(entity.body.velocity,fwdVector);//relVelocity.z;

        while(swingTarget != null && swing.GetInput() != 0)
        {
            entity.ChangeState("Swing");
            entity.actionLock = true;
            entity.body.useGravity = false;
            entity.body.velocity = Vector3.zero;

            Debug.DrawRay(transform.position,fwdVector,Color.yellow);
            upVector = (swingPoint - transform.position).normalized;
            fwdVector = Vector3.Cross(upVector,pole.right);
            float normalAngle = Vector3.SignedAngle(Vector3.ProjectOnPlane(upVector,pole.right),Vector3.up,pole.right);

            float hor = horizontal.GetInput();
            float ver = vertical.GetInput();
            Vector3 input =  moveAxis.forward * ver + moveAxis.right * hor;

            fwdSpeed += Vector3.Dot(fwdVector,input) * swingSpeed.y * Time.fixedDeltaTime;

            fwdSpeed += normalAngle * 10 * Time.fixedDeltaTime;
            fwdSpeed = Mathf.SmoothDamp(fwdSpeed, 0, ref fwdRef, 1, 100,Time.fixedDeltaTime);
            
            Quaternion fwdRot = Quaternion.AngleAxis(fwdSpeed*Time.fixedDeltaTime, pole.right);
            transform.position = (fwdRot*(transform.position-swingPoint)) + swingPoint;
            Quaternion rot = backwards ? Quaternion.LookRotation(fwdVector,upVector) : Quaternion.LookRotation(-fwdVector,upVector);
            transform.rotation = Quaternion.Slerp(transform.rotation,rot,lerp);
            yield return new WaitForFixedUpdate();
        }

        swingTarget = null;
        transform.rotation = backwards ? Quaternion.LookRotation(Vector3.ProjectOnPlane(fwdVector,Vector3.up),Vector3.up) : Quaternion.LookRotation(Vector3.ProjectOnPlane(-fwdVector,Vector3.up),Vector3.up);
        //entity.ChangeState("Idle");
        entity.actionLock = false;
        entity.body.useGravity = true;
        entity.body.velocity = fwdVector.normalized * fwdSpeed/24;
    }

    Collider SwingCheck(float range, float maxAngle)
    {
        Collider[] colliderList = Physics.OverlapSphere(entity.body.position, range);
        Collider target = null; float minAngle = maxAngle;
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
                    target = hitCollider;
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
