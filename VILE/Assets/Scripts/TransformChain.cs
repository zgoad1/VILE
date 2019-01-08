using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformChain : MonoBehaviour {
	public Transform parent;
	[Range(0, 1)] public float lerpFactor = 0.2f;

	private Vector3 offset;

	// Start is called before the first frame update
	void Start() {
		offset = transform.position - parent.position;
	}

	// Update is called once per frame
	void Update() {
		transform.position = Vector3.Lerp(transform.position, parent.position + offset, lerpFactor);
	}

	private void OnDrawGizmosSelected() {
		Gizmos.DrawSphere(transform.position, 1);
	}
}
