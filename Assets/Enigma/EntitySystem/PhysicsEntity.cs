using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enigma
{
    public class PhysicsEntity : Entity
    {
        [System.NonSerialized] public Rigidbody body;
        [System.NonSerialized] public Vector3 normal;
        public bool grounded;
        public bool moving;
        public LayerMask collisionLayers;
        [System.NonSerialized] public float rayLength = 0.125f;

        // Start is called before the first frame update
        void Start()
        {
            body = GetComponentInChildren<Rigidbody>();
            normal = Vector3.up;
        }

        void FixedUpdate()
        {
            grounded = false;
            moving = false;
            ChangeState("Idle");

            //set moving
            if(body.velocity.sqrMagnitude > 0.1f)
            {
                //Debug.Log(body.velocity.sqrMagnitude);
                moving = true;
            }
            //set grounded
            if(GetComponentInChildren<Collider>())
            {
                Collider collider = GetComponentInChildren<Collider>();
                Vector3 downPoint = collider.ClosestPoint(transform.position -Vector3.up*10);
                RaycastHit hit;
                if(Physics.Raycast(downPoint + Vector3.up*0.1f,-Vector3.up,out hit,rayLength,collisionLayers))
                {
                    grounded = true;
                    normal = hit.normal;
                    //body.position -= Vector3.up * Vector3.Distance(downPoint,hit.point);
                }
            }

            if(moving == false && grounded == true && body.velocity != Vector3.zero)
            {
                body.velocity = Vector3.zero;
            }
        }
    }
}

