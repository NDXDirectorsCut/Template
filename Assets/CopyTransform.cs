using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyTransform : MonoBehaviour
{
    public Transform target;
    public bool position;
    public bool rotation;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(target != null)
        {
            if(position)
                transform.position = target.position;
            if(rotation)
                transform.rotation = target.rotation;
        }
    }
}
