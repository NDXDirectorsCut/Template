// Aaron Lanterman, June 19, 2014
using UnityEngine;
using System.Collections;

public class RotateLight : MonoBehaviour {
	Vector3 startingEulerAngles;

	// Use this for initialization
	void Start() {
		startingEulerAngles = transform.eulerAngles;
	}
	
	// Update is called once per frame
	void Update() {
        transform.eulerAngles = 
			startingEulerAngles + new Vector3(0f, 180f * Mathf.Sin(1f * Time.time), 0f);
	}
}
