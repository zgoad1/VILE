using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserFX : MonoBehaviour {

	public float damage = 1;

	[Space]

	private int layerMask;
	[SerializeField] private GameObject sparks;
	[SerializeField] private ParticleSystem parts;
	[SerializeField] private ParticleSystem sparkParts;
	private bool hitting;

	private void Reset() {
		layerMask = 1 << LayerMask.NameToLayer("Solid") | 1 << LayerMask.NameToLayer("Characters");
		parts = GetComponent<ParticleSystem>();
		sparkParts = GetComponentsInChildren<ParticleSystem>()[1];
		sparks = sparkParts.gameObject;
	}

	private void Start() {
		Reset();
	}

	// Update is called once per frame
	void Update () {
		RaycastHit hit;
		// this has to be done with a raycast so we can get the normal and the exact hit point
		if(hitting && Physics.Raycast(transform.position, transform.forward, out hit, layerMask)) {
			// play sparks
			sparkParts.Play();
			// position sparks at hit
			sparks.transform.position = hit.point;
			sparks.transform.forward = hit.normal;
		} else {
			// stop sparks
			if(!sparkParts.isPlaying) sparkParts.Stop();
		}

		// debug
		if(Input.GetKeyDown(KeyCode.Space)) ShootLaser();
	}

	private void OnParticleCollision(GameObject other) {
		hitting = true;
		Controllable target = other.GetComponent<Controllable>();
		if(target != null) {
			// damage
		}
		StopSparks();
	}

	private void ShootLaser() {
		parts.Play();
	}

	private void StopSparks() {
		StopCoroutine("WaitForStopHit");
		StartCoroutine("WaitForStopHit");
	}

	// if no particles hit anything for half a second, stop playing the sparks
	private IEnumerator WaitForStopHit() {
		yield return new WaitForSeconds(0.2f);
		hitting = false;
	}
}
