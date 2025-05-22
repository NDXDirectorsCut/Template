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
        [Space(10)]
        public TargetAxis moveAxis;

        void Start()
        {
            entity = GetComponentInChildren<PhysicsEntity>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if(entity.grounded == true)
            {
                if(sprint.GetInput()!=0)
                {
                    StartCoroutine(Move(sprintSpeed,sprintTime));
                }
                else
                {
                    StartCoroutine(Move(walkSpeed,walkTime));
                }
            }
        }

        Vector3 refVelo;
        IEnumerator Move(float moveSpeed, float moveTime)
        {
            float hor = horizontal.GetInput();
            float ver = vertical.GetInput();

            Vector3 direction = moveAxis.forward * ver + moveAxis.right * hor;
            entity.body.velocity = Vector3.SmoothDamp(entity.body.velocity,direction * moveSpeed,ref refVelo, moveTime);
            yield return new WaitForFixedUpdate();
        }


        IEnumerator MoveAir()
        {
            yield return new WaitForFixedUpdate();
        }

    }
}