using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityFix : MonoBehaviour
{
    public Vector3 gravity;

    // Update is called once per frame
    void Update()
    {
         Physics.gravity = gravity;
    }
}
