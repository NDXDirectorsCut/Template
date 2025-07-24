using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Discrot/ShapeTrailSettings")]
public class ShapeTrailSettings : ScriptableObject
{
    public Mesh sourceMesh;
    public int sourceSubMeshIndex;
}
