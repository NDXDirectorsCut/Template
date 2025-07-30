using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuitGame : MonoBehaviour
{
    public float delay = 1;
    public Button button;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    IEnumerator Quit()
    {
        yield return new WaitForSeconds(delay);
        Application.Quit();
    }

    void OnClick()
    {
        StartCoroutine(Quit());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
