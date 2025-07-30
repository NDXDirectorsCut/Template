using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enigma;

public class SwingAction : Action
{
    PhysicsEntity entity;
    Animator animator;
    public ShapeTrailRenderer shapeTrail;
    public Collider swingTarget;
    [Header("Inputs")]
    public ActionInput swing;
    public ActionInput horizontal;
    public ActionInput vertical;

    [Header("Variables")]
    public float targetRange = 20;
    public float targetAngle = 70;
    public float airTime = 0.25f;
    public float swingSpeed;
    public float gravityForce;
    public float damping;
    [Space(10)]
    public TargetAxis moveAxis;
    float startTime = 0;
    float timeInAir;
    

    // Start is called before the first frame update
    void Start()
    {
        entity = GetComponentInChildren<PhysicsEntity>();
        animator = transform.root.GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(entity.grounded == false)
        {
            if(startTime == 0)
            {
                startTime = Time.time;
            }
            timeInAir = Time.time - startTime;
            if(timeInAir > airTime)
            {
                if(swing.GetInputDown()!=0 && entity.actionLock == false)
                {
                    swingTarget = SwingCheck(targetRange,targetAngle);
                    if(swingTarget != null)
                    {   
                        StartCoroutine(SwingBehavior(swingTarget));
                    }
                }
            }
        }
        if(entity.grounded == true)
        {
            startTime = 0;
        }
    }

    IEnumerator SwingBehavior(Collider poleCollider)
    {
        Vector3 swingPoint = swingTarget.ClosestPoint(transform.position);
        Transform pole = poleCollider.transform.parent;
        pole.Find("AttachPoint").position = swingPoint;
        shapeTrail.endAttachment = pole.Find("AttachPoint");

        Vector3 upVector = (swingPoint - transform.position).normalized;
        Vector3 rgtVector = pole.right;
        Vector3 fwdVector = Vector3.Cross(upVector,pole.right);

        Debug.DrawRay(swingPoint,upVector,Color.green);
        Debug.DrawRay(swingPoint,rgtVector,Color.red);
        Debug.DrawRay(swingPoint,fwdVector,Color.blue);
        
        bool backwards = Vector3.Dot(transform.forward,fwdVector)<0 ? false : true;

        float linearSpeed = Vector3.Dot(entity.body.velocity,fwdVector);
        float radius = Vector3.Distance(transform.position,swingPoint);

        float angularSpeed = linearSpeed/radius;

        Debug.Log("Linear: " + linearSpeed + " Angular: " + angularSpeed + " Radius: " + radius);  

        while(swingTarget != null && swing.GetInput() != 0)
        {
            entity.ChangeState("Swing");
            entity.actionLock = true;
            entity.body.useGravity = false;
            entity.body.velocity = Vector3.zero;

            Debug.DrawRay(swingPoint,upVector,Color.green);
            Debug.DrawRay(swingPoint,rgtVector,Color.red);
            Debug.DrawRay(swingPoint,fwdVector,Color.blue);

            upVector = (swingPoint - transform.position).normalized;
            rgtVector = pole.right;
            fwdVector = Vector3.Cross(upVector,pole.right);

            float gravity = Vector3.Dot(-Vector3.up, fwdVector) * gravityForce;

            float hor = horizontal.GetInput();
            float ver = vertical.GetInput();
            Vector3 input =  moveAxis.forward * ver + moveAxis.right * hor;

            float inputForce = Vector3.Dot(input,Vector3.ProjectOnPlane(fwdVector,Vector3.up).normalized) * swingSpeed;
            angularSpeed += inputForce * Time.fixedDeltaTime;
            angularSpeed += gravity * Time.fixedDeltaTime;

            float dampingForce = angularSpeed * damping * Time.fixedDeltaTime;
            angularSpeed -= dampingForce;
            // fwdSpeed += Vector3.Dot(fwdVector,input) * swingSpeed.y * Time.fixedDeltaTime;
            // fwdSpeed += fwdNormal * 10 * Time.fixedDeltaTime;
            float angularSpeedDeg = angularSpeed * Mathf.Rad2Deg * Time.fixedDeltaTime; 
            Quaternion rot = Quaternion.AngleAxis(angularSpeedDeg, pole.right);
            transform.position = (rot*(transform.position-swingPoint)) + swingPoint;

            Quaternion lookRot = backwards ? Quaternion.LookRotation(fwdVector,upVector) : Quaternion.LookRotation(-fwdVector,upVector);
            transform.rotation = lookRot;

            float animSpeed = backwards ? angularSpeed : -angularSpeed;
            animator.SetFloat("ActionFloat",angularSpeedDeg);
            yield return new WaitForFixedUpdate();
        }

        swingTarget = null;
        shapeTrail.endAttachment = null;
        startTime = 0;
        //wires.targetPos = Vector3.zero;
        transform.rotation = backwards ? Quaternion.LookRotation(Vector3.ProjectOnPlane(fwdVector,Vector3.up),Vector3.up) : Quaternion.LookRotation(Vector3.ProjectOnPlane(-fwdVector,Vector3.up),Vector3.up);
        //entity.ChangeState("Idle");
        entity.actionLock = false;
        entity.body.useGravity = true;
        linearSpeed = radius*angularSpeed;
        entity.body.velocity = fwdVector.normalized * linearSpeed;// + rgtVector.normalized * rgtSpeed/24;
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
