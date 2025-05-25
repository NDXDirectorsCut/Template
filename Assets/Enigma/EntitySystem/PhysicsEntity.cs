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
        public LayerMask collisionLayers;

        // Start is called before the first frame update
        void Start()
        {
            body = GetComponentInChildren<Rigidbody>();
            normal = Vector3.up;
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
                if(Physics.Raycast(downPoint + Vector3.up*0.1f,-Vector3.up,out hit,0.125f,collisionLayers))
                {
                    grounded = true;
                    //body.position -= Vector3.up * Vector3.Distance(downPoint,hit.point);
                }
            }
        }
    }
}

