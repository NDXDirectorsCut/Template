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
        public float turnTime;

        public Transform orbitTarget;
        public float orbitDistance;
        public Vector2 orbitTime;
        
        public Transform lookTarget;
        public float lookTime;

        public LayerMask collisionLayers;

        float hor,ver;
        
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

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

            if(orbitTarget!= null)
            {
                hor = horizontal.GetInput();
                ver = vertical.GetInput();
                StartCoroutine(Turn(hor,ver,orbitTarget));
                StartCoroutine(Collide(orbitTarget.position));
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

        Vector2 turnVelo;
        Vector2 angles;
        IEnumerator Turn(float turnX,float turnY, Transform orbitTarget)
        {
            angles.x = Mathf.SmoothDamp(angles.x,turnX*turnSpeed,ref turnVelo.x,turnTime, 180, Time.fixedDeltaTime);
            Quaternion xRot = Quaternion.AngleAxis(angles.x,Vector3.up);
            transform.position = 
                (xRot*(transform.position-orbitTarget.position)) + orbitTarget.position;
            transform.forward = 
                xRot * transform.forward;

            angles.y = Mathf.SmoothDamp(angles.y,turnY*turnSpeed,ref turnVelo.y,turnTime, 180, Time.fixedDeltaTime);
            Quaternion yRot = Quaternion.AngleAxis(angles.y,transform.right);
            transform.position = (yRot *(transform.position-orbitTarget.position))
                + orbitTarget.position;
            transform.forward = yRot*transform.forward;
            
            yield return new WaitForFixedUpdate();
        }

        IEnumerator Collide(Vector3 castPos)
        {
            RaycastHit hit;
            if(Physics.Raycast(castPos,-transform.forward,out hit,orbitDistance,collisionLayers))
            {
                transform.position = hit.point+transform.forward*0.125f;
            }
            yield return new WaitForFixedUpdate();
        }
    }
}