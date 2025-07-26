using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeString : MonoBehaviour
{
    public bool enabled = false;
    bool tracker = false;
    public Transform target;

    public GameObject realString;
    public LineRenderer fakeString;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(tracker != enabled)
        {
            tracker = enabled;
            StringLogic();
        }
        if(enabled == true)
        {
            fakeString.SetPosition(0,transform.position);
            if(target != null)
                fakeString.SetPosition(1,target.position);
        }
    }

    void StringLogic()
    {
        if(enabled == true)
        {
            realString.SetActive(false);
            fakeString.enabled = true;
        }
        else
        {
            realString.SetActive(true);
            fakeString.enabled = false;
        }
    }
}
