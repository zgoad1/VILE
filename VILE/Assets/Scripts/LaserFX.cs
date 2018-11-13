using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserFX : MonoBehaviour {

	public float damage = 1;

	[Space]

	private int layerMask;
	[SerializeField] private GameObject sparks;			// the hit point sparks' gameObject
	[SerializeField] private ParticleSystem laserParts;	// laser particles
	[SerializeField] private ParticleSystem sparkParts;	// the sparks at the hit point
	[SerializeField] private ParticleSystem hitParts;	// creates a flashing light where the laser hits the ground
	private bool hitting;
	private AudioManager am;
	private AudioSource laserSound;

	private void Reset() {
		layerMask = 1 << LayerMask.NameToLayer("Solid") | 1 << LayerMask.NameToLayer("Characters");
		laserParts = GetComponent<ParticleSystem>();
		sparkParts = GetComponentsInChildren<ParticleSystem>()[1];
		sparks = sparkParts.gameObject;
		hitParts = GetComponentsInChildren<ParticleSystem>()[2];
		am = FindObjectOfType<AudioManager>();
		laserSound = GetComponent<AudioSource>();
	}

	private void Start() {
		Reset();
	}

	// Update is called once per frame
	void Update () {

		#region hit sparks
		RaycastHit hit;
		// this has to be done with a raycast so we can get the normal and the exact hit point
		// 400 is the approximate length of the laser particle effect
		if(hitting && Physics.Raycast(transform.position, transform.forward, out hit, 400, layerMask)) {
			Debug.Log("Ray collided with " + hit.collider.gameObject.name + "\nDistance: " + (hit.point - transform.position).magnitude);
			// play sparks
			sparkParts.Play();
			// position sparks at hit
			sparks.transform.position = hit.point;
			sparks.transform.forward = hit.normal;
		}
		#endregion
	}

	// set hitting to true until no particles have
	private void OnParticleCollision(GameObject other) {
		//Debug.Log("Particle collided with " + other.name);
		hitting = true;
		Controllable target = other.GetComponent<Controllable>();
		if(target != null) {
			// damage
		}
		StopSparks();
	}

	public void ShootLaser() {
		laserParts.Play();
		sparkParts.Stop();  // hit sparks play automatically as long as they're a child of the laser
							//am.Play("Laser");
		laserSound.Play();
	}

	private void StopSparks() {
		StopCoroutine("WaitForStopHit");
		StartCoroutine("WaitForStopHit");
	}

	// if no particles hit anything for part of a second, stop playing the sparks
	private IEnumerator WaitForStopHit() {
		yield return new WaitForSeconds(0.1f);
		//am.Stop("Laser");
		//laserSound.Stop();
		hitting = false;
	}
}
