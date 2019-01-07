using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// VFX for enemies that have just been killed

public class DyingEnemy : PooledObject {

	/* Standard dying enemy. Explosion particles play and pieces gradually fly off.
	 */

	[SerializeField] private ParticleSystem particles;	// particle effects
	[SerializeField] private Transform pieces;          // parent transform of enemy pieces

	private List<Vector3> initialPositions = new List<Vector3>();
	private List<Quaternion> initialRotations = new List<Quaternion>();

	private void Reset() {
		particles = GetComponentInChildren<ParticleSystem>();
		pieces = transform;
	}

	// Relocate all the pieces and remove their physics
	public override void Restart() {
		gameObject.SetActive(true);
		for(int i = 0; i < initialPositions.Count; i++) {
			Transform t = pieces.GetChild(i);
			Destroy(t.GetComponent<Rigidbody>());
			t.position = initialPositions[i];
			t.rotation = initialRotations[i];
		}
		PlayEffects();
	}

	// Start is called before the first frame update
	void Start() {
		// Set initial variables so we can revert to this upon Restart
		foreach(Transform t in pieces) {
			t.gameObject.AddComponent<BoxCollider>();
			if(t.gameObject.layer == LayerMask.NameToLayer("Default")) {
				t.gameObject.layer = LayerMask.NameToLayer("SmallParts");
			}
			initialPositions.Add(t.position);
			initialRotations.Add(t.rotation);
		}
		PlayEffects();
	}

	private void PlayEffects() {
		particles.Play();
		StartCoroutine("Effects");
	}

	// Blow off pieces gradually as particles play
	private IEnumerator Effects() {
		// Generate an order to blow off the parts (no numbers can be repeated)
		List<int> sequence = new List<int>();
		for(int i = 0; i < initialPositions.Count; i++) {
			sequence.Insert(Random.Range(0, sequence.Count), i);
		}

		// Blow off the pieces at regular time intervals until the particle effect is done
		int uh = 0;
		for(float i = 0; i < particles.main.duration; i += particles.main.duration / initialPositions.Count, uh++) {
			Rigidbody rb = pieces.GetChild(sequence[uh]).gameObject.AddComponent<Rigidbody>();
			rb.AddExplosionForce(Random.Range(1000, 3000), transform.position, 100);
			yield return new WaitForSeconds(particles.main.duration / initialPositions.Count);
		}
	}
}
