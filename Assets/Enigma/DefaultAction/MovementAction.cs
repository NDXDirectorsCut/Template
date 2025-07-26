using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enigma
{
    public class MovementAction : Action
    {
        PhysicsEntity entity;
        [Header("Inputs")]
        public ActionInput horizontal;
        public ActionInput vertical;
        public ActionInput sprint;
        [Header("Variables")]
        public float walkSpeed;
        public float walkAcceleration;
        [Space(5)]
        public float sprintSpeed;
        public float sprintAcceleration;
        [Space(5)]
        public float deceleration;
        public float turnSpeed;
        [Space(10)]
        public TargetAxis moveAxis;
        //
        Vector3 forwardDir;
        float activeMoveSpeed;

        void Start()
        {
            entity = GetComponentInChildren<PhysicsEntity>();
            forwardDir = transform.forward;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            float hor = horizontal.GetInput();
            float ver = vertical.GetInput();
            Vector3 inputDir = moveAxis.forward * ver + moveAxis.right * hor;
            Debug.DrawRay(transform.position,moveAxis.up, Color.green);
            Debug.DrawRay(transform.position,moveAxis.right, Color.red);
            Debug.DrawRay(transform.position,moveAxis.forward, Color.blue);
            inputDir = Vector3.ProjectOnPlane(inputDir,Vector3.up);
            if(entity.grounded == true)
            {
                    if(inputDir.sqrMagnitude>0.1f && entity.ChangeState("Walking")) //Accelerating
                    {
                        if(sprint.GetInput()!=0) //Sprinting
                        {
                            Walk(inputDir, sprintSpeed, sprintAcceleration, turnSpeed);
                        }
                        else
                        {
                            Walk(inputDir, walkSpeed, walkAcceleration, turnSpeed);
                        }
                    }
                    else if(entity.body.velocity.sqrMagnitude>0.1f && entity.ChangeState("Walking")) //Decelerating
                    {
                        Vector3 decelerationForce = entity.body.velocity * deceleration * Time.fixedDeltaTime;
                        entity.body.velocity -= decelerationForce;
                    }
            }
            else if(entity.grounded == false)
            {
                if(inputDir != Vector3.zero && entity.ChangeState("Gliding"))
                {
                    Glide(inputDir,walkSpeed,turnSpeed);
                }
            }
            activeMoveSpeed = Vector3.ProjectOnPlane(entity.body.velocity,Vector3.up).magnitude;
        }

        void Walk(Vector3 dir, float moveSpeed, float accel, float turnSpeed)
        {
            if(entity.GetState() == "Walking")
            {
                float velocity = entity.body.velocity.magnitude;
                float accelerationRatio = 1-(velocity/moveSpeed);

                activeMoveSpeed += accel*accelerationRatio*Time.fixedDeltaTime;

                float turnAngle = Vector3.SignedAngle(forwardDir,dir,Vector3.up);
                float finalTurnAngle = turnAngle * Time.fixedDeltaTime * turnSpeed;
                finalTurnAngle = Mathf.Abs(finalTurnAngle) > Mathf.Abs(turnAngle) ? turnAngle * Time.fixedDeltaTime : finalTurnAngle;
                forwardDir = Quaternion.AngleAxis(finalTurnAngle, Vector3.up) * forwardDir;
                forwardDir = forwardDir.normalized;
                Vector3 applyVelo = new Vector3(forwardDir.x*activeMoveSpeed,entity.body.velocity.y,forwardDir.z*activeMoveSpeed);
                entity.body.velocity = applyVelo;
                transform.forward = forwardDir;
            }
        }


        void Glide(Vector3 dir, float moveSpeed, float turnSpeed)
        {
            moveSpeed = moveSpeed*moveSpeed*2/entity.body.velocity.magnitude;
            entity.body.velocity += dir * moveSpeed * Time.fixedDeltaTime;

            Vector3 planeVelo = Vector3.ProjectOnPlane(entity.body.velocity, Vector3.up);
            float turnAngle = Vector3.SignedAngle(forwardDir,planeVelo,Vector3.up);
            float finalTurnAngle = turnAngle * Time.fixedDeltaTime * turnSpeed;
            finalTurnAngle = Mathf.Abs(finalTurnAngle) > Mathf.Abs(turnAngle) ? turnAngle : finalTurnAngle;

            forwardDir = Quaternion.AngleAxis(finalTurnAngle, Vector3.up) * forwardDir;

            forwardDir = Vector3.ProjectOnPlane(forwardDir,Vector3.up);
            transform.forward = forwardDir;
        }

    }
}