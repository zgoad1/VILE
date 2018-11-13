using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBarrier : MonoBehaviour {

	private void Reset() {
		if(GetComponent<BoxCollider>() == null) gameObject.AddComponent<BoxCollider>();
	}

	private void Start() {
		Reset();
	}
}
