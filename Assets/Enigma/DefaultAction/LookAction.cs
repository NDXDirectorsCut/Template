using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enigma
{
    public class LookAction : Action
    {
        Entity cam;
        [Header("Inputs")]
        public ActionInput horizontal;
        public ActionInput vertical;
        [Header("Variables")]
        public float turnSpeed;

        public Transform orbitTarget;
        public float orbitDistance;
        public Vector2 orbitTime;
        
        public Transform lookTarget;
        public float lookTime;
        

        void FixedUpdate()
        {
            if(orbitTarget!= null)
            {
                StartCoroutine(Orbit(orbitTarget, orbitDistance, orbitTime));
            }

            if(lookTarget != null)
            {
                StartCoroutine(Look(lookTarget, lookTime));
            }

        }

        Vector3 horVelocity;
        float verVelocity;
        IEnumerator Orbit(Transform target, float distance, Vector2 time)
        {
            Vector3 targetPos = target.position - transform.forward * distance;

            Vector3 horPosition = Vector3.ProjectOnPlane(transform.position,Vector3.up);
            Vector3 horTarget = Vector3.ProjectOnPlane(targetPos,Vector3.up);
            float verPosition = transform.position.y;
            float verTarget = targetPos.y;
            
            horPosition = Vector3.SmoothDamp(horPosition,horTarget,ref horVelocity, time.x, 25, Time.fixedDeltaTime);
            verPosition =   Mathf.SmoothDamp(verPosition,verTarget,ref verVelocity, time.y, 25, Time.fixedDeltaTime);

            Vector3 finalPos = new Vector3(horPosition.x,verPosition,horPosition.z);
            transform.position = finalPos;
            yield return new WaitForFixedUpdate();
        }

        Vector3 lookVelocity;
        IEnumerator Look(Transform target, float time)
        {
            Vector3 targetDir = -(transform.position - target.position).normalized;
            Debug.DrawRay(transform.position,targetDir,Color.blue);
            transform.forward = Vector3.SmoothDamp(transform.forward, targetDir,ref lookVelocity, time, 180, Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        IEnumerator Turn(transform orbitTarget,Transform lookTarget)
        {
            
        }
    }
}