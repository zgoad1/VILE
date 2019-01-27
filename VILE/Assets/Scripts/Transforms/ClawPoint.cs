using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Follows the parent transform's position using lerp.
// Also draws cool electricity effects maybe if I can do it
public class ClawPoint : TransformChain {

	[Range(0, 1)] public float tightness = 0.2f;
	public Transform follow;
	//public LerpFollow[] next = new LerpFollow[1];
	//[Tooltip("Only important for Tess Claw points. Whether this is the starting point of the chain.")]
	//public bool root = false;

	private void OnEnable() {
		transform.position = follow.position;
	}

	// Update is called once per frame
	void Update() {
		transform.position = Vector3.Lerp(transform.position, follow.position, tightness * 60 * Time.deltaTime);
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(transform.position, 1);
		foreach(ClawPoint n in next) {
			Gizmos.DrawLine(transform.position, n.transform.position);
		}
	}
}
