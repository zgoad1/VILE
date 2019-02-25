using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatAnimation : MonoBehaviour {

	public float amp = 0.2f;
	public float per = 1f;
	public float ps = 0f;

	private Vector3 iPos;
	private Vector3 offset = Vector3.zero;

	// Start is called before the first frame update
	void Start() {
		iPos = transform.position;
	}

	// Update is called once per frame
	void Update() {
		offset.y = amp * Mathf.Sin(2 * Mathf.PI / per * Time.time + ps);
		transform.position = iPos + offset;
	}
}
