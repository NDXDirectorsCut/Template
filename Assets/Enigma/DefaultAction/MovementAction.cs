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
        public float walkTime;
        [Space(5)]
        public float sprintSpeed;
        public float sprintTime;
        [Space(5)]
        public float turnSpeed;
        [Space(10)]
        public TargetAxis moveAxis;
        //
        Vector3 forwardDir;

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
            
            if(entity.grounded == true && entity.ChangeState("Idle"))
            {
                if(sprint.GetInput()!=0)
                {
                    StartCoroutine(Move(inputDir, sprintSpeed,sprintTime));
                }
                else
                {
                        StartCoroutine(Move(inputDir, walkSpeed,walkTime));
                }
            }
            else if(entity.grounded == false && entity.ChangeState("Idle"))
            {
                if(inputDir != Vector3.zero)
                {
                    StartCoroutine(MoveAir(inputDir,walkSpeed*walkSpeed*2/entity.body.velocity.magnitude));
                }
            }
        }

        float refVelo;
        IEnumerator Move(Vector3 moveDir, float moveSpeed, float moveTime)
        {
            float turnAngle = Vector3.SignedAngle(forwardDir,moveDir,Vector3.up) * Time.fixedDeltaTime ;
            float finalTurnAngle = turnAngle * turnSpeed;
            finalTurnAngle = Mathf.Abs(finalTurnAngle) > Mathf.Abs(turnAngle) ? turnAngle : finalTurnAngle;

            forwardDir = Quaternion.AngleAxis(finalTurnAngle, Vector3.up) * forwardDir;
            forwardDir = forwardDir.normalized;
            float speed = Mathf.SmoothDamp(entity.body.velocity.magnitude,moveSpeed*moveDir.magnitude,ref refVelo, moveTime,100,Time.fixedDeltaTime);
            Vector3 velocity = forwardDir*speed;
            entity.body.velocity = new Vector3(velocity.x, entity.body.velocity.y, velocity.z); //Vector3.SmoothDamp(entity.body.velocity,entity.body.velocity.normalized * moveSpeed,ref refVelo, moveTime);
            transform.forward = forwardDir;
            yield return new WaitForFixedUpdate();
        }


        IEnumerator MoveAir(Vector3 moveDir, float moveSpeed)
        {
            entity.body.velocity += moveDir * moveSpeed * Time.fixedDeltaTime;

            Vector3 planeVelo = Vector3.ProjectOnPlane(entity.body.velocity, Vector3.up);
            float turnAngle = Vector3.SignedAngle(forwardDir,planeVelo,Vector3.up);
            float finalTurnAngle = turnAngle * Time.fixedDeltaTime * turnSpeed;
            finalTurnAngle = Mathf.Abs(finalTurnAngle) > Mathf.Abs(turnAngle) ? turnAngle : finalTurnAngle;

            forwardDir = Quaternion.AngleAxis(finalTurnAngle, Vector3.up) * forwardDir;

            forwardDir = Vector3.ProjectOnPlane(forwardDir,Vector3.up);
            transform.forward = forwardDir;
            yield return new WaitForFixedUpdate();
        }

    }
}