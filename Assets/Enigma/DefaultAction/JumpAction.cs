using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enigma
{
    public class JumpAction : Action
    {
        PhysicsEntity entity;
        [Header("Inputs")]
        public ActionInput jump;
        [Header("Variables")]
        public float jumpForce;
        bool landed = true;
        bool jumping = false;
        float landTime;
        bool additiveJump;
        public float additiveJumpForce;
        public float jumpTime;

        float rayHold;

        // Start is called before the first frame update
        void Start()
        {
            entity = GetComponentInChildren<PhysicsEntity>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if(entity.grounded == true && entity.GetState() != "Jump")
            {  
                if(landed == false)
                {
                    landed = true;
                    landTime = Time.time;
                }
                if(Time.time-landTime>0.1f && jump.GetInput()!=0 && entity.ChangeState("Jump"))
                {
                    StartCoroutine(Jump(entity.normal,jumpForce,additiveJumpForce));
                }
            }
            if(jumping == true && entity.body.velocity.y > 0.2f)
            {
                jumping = true;
                entity.ChangeState("Jump");
            }
            else
            {
                jumping = false;
            }
        }

        IEnumerator Jump(Vector3 dir, float force, float additiveForce = 0)
        {
            rayHold = entity.rayLength;
            yield return new WaitForFixedUpdate();
            landed = false;
            jumping = true;
            entity.grounded = false;
            entity.rayLength = 0;
            entity.body.velocity += dir*force;
            float startTime = Time.time;
            float time = 0;
            while(time<jumpTime && entity.grounded == false && jump.GetInput()!=0 && entity.ChangeState("Jump"))
            {
                if(time>0.05f)
                {
                    entity.rayLength = rayHold/2;
                    entity.body.velocity += dir*additiveForce*Time.fixedDeltaTime;
                    jumping = true;
                }
                time = Time.time-startTime;
                yield return new WaitForFixedUpdate();
            }
            entity.rayLength = rayHold;
        }
    }
}