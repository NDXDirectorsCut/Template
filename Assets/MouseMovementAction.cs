using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enigma;

public class MouseMovementAction : Action
{
    PhysicsEntity entity;
    Animator animator;
    [Header("Inputs")]
    public ActionInput charge;
    public ActionInput horizontal;
    [Header("Variables")]
    public float moveSpeed;
    public float turnSpeed;
    public float deceleration;
    public TargetAxis moveAxis;
    float holdTime;
    float startTime = 0;
    float horInput;

    // Start is called before the first frame update
    void Start()
    {
        entity = GetComponentInChildren<PhysicsEntity>();
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        horInput = horizontal.GetInput();
        if(charge.GetInput()!=0 && playerControlled == true)
        {
            if(entity.ChangeState("Charging"))
            {
                if(startTime == 0)
                {
                    startTime = Time.time;
                }
                holdTime = Time.time - startTime;
                animator.SetFloat("ActionFloat",Mathf.Clamp(holdTime*4,0,6));
            }
        }
        if(holdTime!=0 && charge.GetInput()==0)
        {
            if(entity.ChangeState("Moving"))
            {
                StartCoroutine(Move(moveSpeed,Mathf.Clamp(holdTime*2,0,3),turnSpeed));
            }
            holdTime = 0;
            startTime = 0;
        }
    }

    IEnumerator Move(float speed,float charge, float turnSpeed)
    {
        entity.body.velocity = transform.forward*speed*charge;
        while(entity.grounded == true && entity.body.velocity.magnitude>0.1f)
        {
            Vector3 decelerationForce = entity.body.velocity * deceleration * Time.fixedDeltaTime;
            entity.body.velocity -= decelerationForce;
            float angle = horInput*turnSpeed;
            entity.body.velocity = 
                Quaternion.AngleAxis(angle,Vector3.up) * entity.body.velocity;
            transform.forward = entity.body.velocity;
            yield return new WaitForFixedUpdate();
        }
    }
}
