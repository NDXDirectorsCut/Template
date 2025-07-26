using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class TrailPoint
{
    public Vector3 position;
    public Vector3 velocity;
    public float startTime;
};

public class ShapeTrailRenderer : MonoBehaviour
{
    [Header("Shape")]
    public Mesh sourceShape;
    public MeshFilter meshRender;
    Mesh trailMesh;
    public int maxPoints;
    public AnimationCurve widthCurve;
    public float widthMultiplier = 1;
    public float length = 2;

    //Compute
    public ComputeShader compute;
    ComputeBuffer inputVertexBuffer;
    ComputeBuffer segmentBuffer;
    ComputeBuffer outputVertexBuffer;
    ComputeBuffer outputIndexBuffer;

    struct VertexData
    {
        public Vector3 position;
        public Vector3 normal;
    }

    int vertexStride = Marshal.SizeOf(typeof(VertexData));
    int indexStride = sizeof(int);


    [Header("Physics")]
    public bool usePhysics;

    public Transform endAttachment;
    [Range(0,1)]public float errorCorrection;

    public float startSpeed;
    public Vector3 force;
    public float spring;
    public float damp;
    public bool collide;
    public float collisionRadius;
    public float lifetime;
    //public float minVertexDistance = 0.1f;
    
    List<TrailPoint> trail = new List<TrailPoint>();

    // Start is called before the first frame update
    void Start()
    {

    }

    void GenerateMesh(int points)
    {
        Vector3[] vertexPositions = sourceShape.vertices;
        Vector3[] normals = sourceShape.normals;

        int vertexCount = vertexPositions.Length;
        int outputVertCount = vertexCount*points;
        VertexData[] inputVertices = new VertexData[vertexCount];
        
        for(int i=0; i<vertexCount; i++)
        {
            inputVertices[i] = new VertexData 
            {
                position = vertexPositions[i],
                normal = normals[i]
            };
        }

        Vector3[] segmentPoints = new Vector3[points];
        for(int i=0; i<points; i++)
        {
            segmentPoints[i] = transform.InverseTransformPoint(trail[i].position);
        }

        int totalQuads = (vertexCount - 1) * (points - 1);
        int totalIndices = totalQuads * 6;

        inputVertexBuffer = new ComputeBuffer(vertexCount, vertexStride);
        segmentBuffer = new ComputeBuffer(points, sizeof(float)*3);
        outputVertexBuffer = new ComputeBuffer(outputVertCount, vertexStride);
        outputIndexBuffer = new ComputeBuffer(totalIndices, indexStride);

        inputVertexBuffer.SetData(inputVertices);
        segmentBuffer.SetData(segmentPoints);

        int kernel = compute.FindKernel("ShapeTrailMain");
        compute.SetInt("vertexCount", vertexCount);
        compute.SetInt("points", points);
        compute.SetFloat("radius", collisionRadius*widthMultiplier);

        compute.SetBuffer(kernel, "inputVertices", inputVertexBuffer);
        compute.SetBuffer(kernel, "segmentPositions", segmentBuffer);
        compute.SetBuffer(kernel, "outputVertices", outputVertexBuffer);
        compute.SetBuffer(kernel, "outputIndices", outputIndexBuffer);

        int threadGroups = Mathf.CeilToInt(outputVertCount / 128f);
        compute.Dispatch(kernel, threadGroups, 1, 1);

         // Read back modified data
        VertexData[] outputVertices = new VertexData[outputVertCount];
        int[] outputIndices = new int[totalIndices];

        outputVertexBuffer.GetData(outputVertices);
        outputIndexBuffer.GetData(outputIndices);

        Vector3[] outPositions = new Vector3[outputVertCount];
        Vector3[] outNormals = new Vector3[outputVertCount];

        for (int i=0; i<outputVertCount; i++)
        {
            outPositions[i] = outputVertices[i].position;
            outNormals[i] = outputVertices[i].normal;
        }

        trailMesh = new Mesh();
        trailMesh.vertices = outPositions;
        trailMesh.normals = outNormals;
        trailMesh.triangles = outputIndices;

        meshRender.mesh = trailMesh;

        inputVertexBuffer.Release();
        segmentBuffer.Release();
        outputVertexBuffer.Release();
        outputIndexBuffer.Release();
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

    void Update()
    {
        if(trail.Count>1)
            GenerateMesh(trail.Count);
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

        if(id==0)
        {
            trail[0].position = transform.position;
            return;
        }
        float segmentLength = length/(trail.Count - 1);
        if(endAttachment == null)
        {
            if(id>0)
            {
                Vector3 springForce;
                Vector3 dampForce;
                Vector3 target;

                    //previous point
                Vector3 prevPos = trail[id-1].position;
                Vector3 toPrev = (prevPos - trail[id].position);
                target = prevPos - toPrev.normalized * segmentLength;
                springForce = (target - trail[id].position) * spring * Time.fixedDeltaTime;
                    
                trail[id].velocity += springForce;
                    
                //next point
                if(id+1 < trail.Count)
                {
                    Vector3 nextPos = trail[id+1].position;
                    Vector3 toNext = (nextPos - trail[id].position);
                    target = nextPos - toNext.normalized * segmentLength; 

                    springForce = (target - trail[id].position) * spring * Time.fixedDeltaTime;
                    trail[id].velocity += springForce;
                }

                //damp
                dampForce = trail[id].velocity * damp * Time.fixedDeltaTime;
                trail[id].velocity -= dampForce;

                trail[id].velocity += force * Time.fixedDeltaTime;
                Vector3 tempPos = trail[id].position + trail[id].velocity * Time.fixedDeltaTime;

                if (collide == true)
                {
                    RaycastHit hit;
                    Vector3 moveDir = trail[id].velocity.normalized;

                    if (Physics.Raycast(trail[id].position, moveDir, out hit, collisionRadius))
                    {
                        trail[id].position = hit.point + hit.normal * collisionRadius;
                        trail[id].velocity = Vector3.zero;
                    }
                    else
                    {
                        trail[id].position = tempPos;
                    }

                    Vector3 delta = trail[id].position - trail[id-1].position;
                    float error = delta.magnitude - segmentLength;
                    trail[id].position -= delta.normalized*error*errorCorrection;
                    return;
                }
            }
        }
        if(endAttachment != null)
        {
            Vector3 dir = endAttachment.position - transform.position;
            segmentLength = dir.magnitude/trail.Count;
            Vector3 targetPos = transform.position + dir.normalized*segmentLength*id;
            
            trail[id].position = Vector3.Lerp(trail[id].position,targetPos,0.25f);//trail[id].velocity;
        }
        //end

        if(endAttachment != null && id == trail.Count-1)
        {
            trail[id].position = endAttachment.position;
        }
    }

}
