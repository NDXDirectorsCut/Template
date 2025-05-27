using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enigma;

public class SimpleAnim : MonoBehaviour
{
    Animator animator;
    PhysicsEntity entity;
    string state;
    // Start is called before the first frame update
    void Start()
    {
        entity = transform.root.GetComponentInChildren<PhysicsEntity>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(entity.GetState() != state)
        {
            state = entity.GetState();
            animator.CrossFadeInFixedTime(state,.25f);
        }
        animator.SetFloat("Velocity",entity.body.velocity.magnitude);
    }
}
