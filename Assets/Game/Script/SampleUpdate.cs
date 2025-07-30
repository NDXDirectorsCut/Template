using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SampleUpdate : MonoBehaviour
{
    public TMP_Text UIElement;
    public Slider slider;
    // Start is called before the first frame update
    void Start()
    {
        UIElement = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        UIElement.text = slider.value.ToString();
    }
}
