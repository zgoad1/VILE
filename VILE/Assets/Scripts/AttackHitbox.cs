using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**Damages each character it collides with exactly once between activations.
 */

[RequireComponent(typeof(BoxCollider))]
public class AttackHitbox : MonoBehaviour {

	protected new BoxCollider collider;
	protected List<Controllable> hits = new List<Controllable>();   // characters currently in the hitbox
	public float power = 5;

	private void Reset() {
		collider = GetComponent<BoxCollider>();
		collider.isTrigger = true;
	}

	protected void OnTriggerEnter(Collider other) {
		Controllable character = other.gameObject.GetComponent<Controllable>();
		if(character != null) {
			character.Damage(power);
			hits.Add(character);
		}
	}

	public void Activate() {
		collider.enabled = true;
	}

	public void Deactivate() {
		hits.Clear();
		collider.enabled = false;
	}
}
