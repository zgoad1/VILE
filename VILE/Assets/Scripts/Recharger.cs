using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recharger : MonoBehaviour {
	public ParticleSystem electricityParticles;
	public float radius = 25;

	private Vector3 newScale = Vector3.one;

	private void Reset() {
		electricityParticles = GetComponent<ParticleSystem>();
	}

	// Start is called before the first frame update
	void Start() {
		Reset();
	}

	// Update is called once per frame
	void Update() {
		float dist = Vector3.Distance(transform.position, GameController.player.transform.position);
		if(dist <= radius) {
			GameController.player.stamina += Time.deltaTime * 60;
			// make stamina bar flash
			// sound effect
			transform.LookAt(GameController.player.camLook);
			if(!electricityParticles.isPlaying) {
				electricityParticles.Play();
			}
			newScale.z = dist / radius / 3f;
			transform.localScale = newScale;
		} else {
			electricityParticles.Stop();
		}
	}
}
