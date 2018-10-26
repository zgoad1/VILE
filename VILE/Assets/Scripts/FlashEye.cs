using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashEye : MonoBehaviour {

	FlashEyeBall ball;

	void Reset() {
		ball = GetComponentInChildren<FlashEyeBall>();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
