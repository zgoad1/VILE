using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformFollow : TransformChain {
	public bool drawGizmos = false;
	[Range(0, 1)] public float tightness = 0.2f;

	private Vector3 offset;
	private Quaternion iParentRotation;

	// Start is called before the first frame update
	void Start() {
		offset = transform.position - next[0].transform.position;
		iParentRotation = next[0].transform.rotation;
	}

	// Update is called once per frame
	void Update() {
		transform.position = Vector3.Lerp(transform.position, next[0].transform.position + offset, tightness * 60 * Time.deltaTime);
		
	}

	private void OnDrawGizmos() {
		if(drawGizmos) Gizmos.DrawSphere(transform.position, 1);
	}
}
