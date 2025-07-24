using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MenuEffect : MonoBehaviour
{
    public EventSystem eventSystem;
    public Transform curtain;
    public float springForce;
    public float dampForce;
    Vector3 curtainVelocity;
    Vector3 calculatedPos;

    float[] curtainPositions = 
    {
        0.002f,
        0.0f,
        -0.002f,
        -0.004f
    };
    [Range(0,1)] public float lightTransition = 0.1f;
    [Range(0,1)] public float sizeTransition = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        calculatedPos = curtain.localPosition;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        for(int i=0; i<transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Vector3 scale = child.GetComponent<RectTransform>().localScale;
            Image image = child.GetChild(0).GetComponent<Image>();
            if(child.gameObject != eventSystem.currentSelectedGameObject)
            {   
                image.color = Color.Lerp(image.color,Color.clear,lightTransition);
                child.GetComponent<RectTransform>().localScale =
                    Vector3.Lerp(scale,Vector3.one*0.5f,sizeTransition);
            }
            else
            {

                image.color = Color.Lerp(image.color,Color.white,lightTransition);
                child.GetComponent<RectTransform>().localScale = 
                    Vector3.Lerp(scale,Vector3.one,sizeTransition);
                //Curtain
                Vector3 targetPos = new Vector3(curtainPositions[i],0,0);
                Vector3 currentPos = calculatedPos;

                Vector3 springVector = (targetPos - calculatedPos);
                curtainVelocity += springVector * springForce * Time.deltaTime;
                curtainVelocity -= curtainVelocity * dampForce * Time.deltaTime;
                calculatedPos += curtainVelocity * Time.deltaTime;
                curtain.localPosition = calculatedPos;
                Debug.Log(targetPos + "-/" + calculatedPos);
            }
        
        }
    }

}
