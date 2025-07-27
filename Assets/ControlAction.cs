using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enigma;

public class ControlAction : Action
{
    public Entity controlled;
    public LookAction cameraControl;
    public ShapeTrailRenderer leftString;
    public ShapeTrailRenderer rightString;
    PhysicsEntity entity;

    [Header("Inputs")]
    public ActionInput control;
    [Header("Variables")]
    public float controlRange;
    public float controlAngle;
    // public float moveSpeed;
    // public float turnSpeed
    // public float deceleration;
    // public TargetAxis moveAxis;

    Entity EntityCheck(float range, float maxAngle)
    {
        Collider[] colliderList = Physics.OverlapSphere(entity.body.position, controlRange);
        Collider target = null; float minAngle = maxAngle;
        foreach(var hitCollider in colliderList)
        {
            Entity proposed = hitCollider.transform.root.GetComponentInChildren<Entity>();
            if(proposed != null && proposed != entity)
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
            return target.transform.root.GetComponentInChildren<Entity>();
        }

        return null;
    }

    // Start is called before the first frame update
    void Start()
    {
        entity = GetComponentInChildren<PhysicsEntity>();
    }

    // Update is called once per frame
    void Update()
    {
        if(control.GetInputDown()!=0 && controlled == null && entity.actionLock == false && entity.ChangeState("Controlling"))
        {
            controlled = EntityCheck(controlRange,controlAngle);

            leftString.endAttachment = controlled.transform;
            rightString.endAttachment = controlled.transform;

            Action[] entityActions = controlled.gameObject.GetComponents<Action>();
            foreach(Action action in entityActions)
            {
                action.playerControlled = true;
            }
            if(cameraControl!=null)
            {
                cameraControl.orbitTarget = controlled.transform.Find("CameraTarget");
                cameraControl.lookTarget = controlled.transform.Find("CameraTarget");
                
            }
            entity.actionLock = true;
            entity.body.velocity = Vector3.zero;
            return;
        }
        if(control.GetInputDown()!=0 && controlled != null)
        {
            leftString.endAttachment = null;
            rightString.endAttachment = null;

            Action[] entityActions = controlled.gameObject.GetComponents<Action>();
            foreach(Action action in entityActions)
            {
                action.playerControlled = false;
            }
            controlled = null;
            if(cameraControl != null)
            {
                cameraControl.orbitTarget = transform.Find("CameraTarget");
                cameraControl.lookTarget = transform.Find("CameraTarget");
            }
            entity.actionLock = false;
        }
    }
}
