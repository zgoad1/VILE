using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformFollow : MonoBehaviour {
	public bool drawGizmos = false;
	public Transform parent;
	[Range(0, 1)] public float tightness = 0.2f;

	private Vector3 offset;

	// Start is called before the first frame update
	void Start() {
		offset = transform.position - parent.position;
	}

	// Update is called once per frame
	void Update() {
		transform.position = Vector3.Lerp(transform.position, parent.position + offset, tightness * 60 * Time.deltaTime);
	}

	private void OnDrawGizmos() {
		if(drawGizmos) Gizmos.DrawSphere(transform.position, 1);
	}
}
