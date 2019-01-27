using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBarrier : MonoBehaviour {

	private void Reset() {
		BoxCollider bc = GetComponent<BoxCollider>();
		if(bc == null) bc = gameObject.AddComponent<BoxCollider>();
		bc.size = new Vector3(bc.size.x * 0.4f, bc.size.y, bc.size.z * 0.4f);
	}

	private void Start() {
		Reset();
	}
}
