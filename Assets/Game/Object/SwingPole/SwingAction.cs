using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enigma;

public class SwingAction : Action
{
    PhysicsEntity entity;
    [Header("Inputs")]
    public ActionInput swing;
    [Header("Variables")]
    // Start is called before the first frame update
    void Start()
    {
        entity = GetComponentInChildren<PhysicsEntity>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(entity.grounded == false)
        {
            if(swing.GetInput()!=0 && entity.ChangeState("Swing"))
            {

            }
        }
    }

    IEnumerator SwingCheck()
    {
        
    }
}
