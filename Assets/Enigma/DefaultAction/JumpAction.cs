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
                if(jump.GetInput()!=0 && entity.ChangeState("Jump"))
                {
                    StartCoroutine(Jump(jumpForce,Vector3.up));
                }
            }
        }

        IEnumerator Jump(float force, Vector3 dir)
        {
            entity.grounded = false;
            entity.body.velocity += dir*force;
            yield return null;
        }
    }
}