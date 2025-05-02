using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enigma
{
    public class PhysicsEntity : Entity
    {
        Rigidbody body;
        public bool grounded;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            //set grounded
            grounded = false;
            if(GetComponentInChildren<Collider>())
            {
                Collider collider = GetComponentInChildren<Collider>();
                Vector3 downPoint = collider.ClosestPoint(transform.position -Vector3.up*10);
                RaycastHit hit;
                if(Physics.Raycast(downPoint + Vector3.up*0.1f,-Vector3.up,out hit,0.25f,collisionLayers))
                {
                    grounded = true;
                    body.position -= Vector3.up * Vector3.Distance(downPoint,hit.point);
                }
            }
        }
    }
}

