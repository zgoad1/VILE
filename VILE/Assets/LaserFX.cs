using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserFX : MonoBehaviour {

	int layerMask;
	GameObject sparks;
	ParticleSystem sparkParts;

	private void Reset() {
		layerMask = 1 << LayerMask.NameToLayer("Solid") | 1 << LayerMask.NameToLayer("Characters");
		sparkParts = GetComponentsInChildren<ParticleSystem>()[1];
		sparks = sparkParts.gameObject;
	}

	private void Start() {
		Reset();
	}

	// Update is called once per frame
	void Update () {
		RaycastHit hit;
		if(Physics.Raycast(transform.position, transform.forward, out hit, layerMask)) {
			// play sparks
			sparkParts.Play();
			// position sparks at hit
			sparks.transform.position = hit.point;
			sparks.transform.forward = hit.normal;
		} else {
			// stop sparks
			if(!sparkParts.isPlaying) sparkParts.Stop();
		}
	}
}
