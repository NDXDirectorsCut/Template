using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UIFix : MonoBehaviour
{
    public GameObject selected;
    public EventSystem eventSystem;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(eventSystem.currentSelectedGameObject != selected && eventSystem.currentSelectedGameObject != null)
        {
            selected = eventSystem.currentSelectedGameObject;
        }
        if(eventSystem.currentSelectedGameObject == null)
        {
            eventSystem.SetSelectedGameObject(selected);
        }
    }
}
