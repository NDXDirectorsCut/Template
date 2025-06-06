using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailPoint
{
    public Vector3 position;
    public Vector3 velocity;
    public float startTime;
};

public class ShapeTrailRenderer : MonoBehaviour
{

    public Mesh shape;
    public float startSpeed;
    public Vector3 force;
    public float spring;
    public float damp;
    public bool collide;
    public float collisionRadius;
    public float lifetime;
    public AnimationCurve widthCurve;
    public float widthMultiplier = 1;
    public float length = 2;
    //public float minVertexDistance = 0.1f;
    public int maxPoints;

    List<TrailPoint> trail = new List<TrailPoint>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        StartCoroutine(Emit());

        for(int i=0; i<trail.Count; i++)
        {
            PointBehavior(i);
        }

        if(Input.GetKey(KeyCode.R))
        {
            trail.Clear();
        }
    }

    IEnumerator Emit()
    {
        while(maxPoints == 0 || trail.Count < maxPoints)
        {
            TrailPoint newPoint = new TrailPoint();
            newPoint.velocity = transform.up * startSpeed;
            newPoint.position = trail.Count > 0 ? trail[trail.Count-1].position : transform.position;
            newPoint.startTime = Time.time;
            trail.Add(newPoint);
            yield return new WaitForSeconds(lifetime/maxPoints);
        }
    }

    Vector3 refVelo;
    void PointBehavior(int id)
    {
        //Debug
        if(id+1 < trail.Count)
        {
            Vector3 dir = trail[id+1].position - trail[id].position;
            Debug.DrawRay(trail[id].position,dir,Color.red);
            Debug.DrawRay(trail[id].position,Vector3.Cross(dir,Vector3.right),Color.blue);
        }

        trail[0].position = transform.position;

        if(id>0)
        {
            trail[id].velocity += force * Time.fixedDeltaTime;
            Vector3 dir = (trail[id].position-trail[id-1].position);
            Vector3 targetPos = trail[id-1].position + dir.normalized*length/(trail.Count-1);
            Vector3 springVector = (targetPos - trail[id].position);
            trail[id].velocity -= springVector * spring * Time.fixedDeltaTime;
            trail[id].velocity -= trail[id].velocity * damp * Time.fixedDeltaTime;
            trail[id].position += trail[id].velocity * Time.fixedDeltaTime;

            Vector3 nDir = (trail[id].position-trail[id-1].position);
            targetPos = trail[id-1].position + nDir.normalized*length/(trail.Count-1);
            trail[id].position = targetPos;

            if(collide == true)
            {
                RaycastHit hit;
                if(Physics.Raycast(trail[id].position,trail[id].velocity,out hit,collisionRadius))
                {
                    trail[id].velocity = Vector3.ProjectOnPlane(trail[id].velocity,hit.normal);
                    trail[id].position = hit.point+hit.normal*collisionRadius;
                }
            }

            //trail[id].velocity = Vector3.SmoothDamp(trail[id].velocity,Vector3.zero, ref refVelo, .5f);
        }
    }

}
