using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**Continuously exerts damage on everything that's not invincible (e.g. due to grace period)
 */

[RequireComponent(typeof(BoxCollider))]
public class AttackHitbox : MonoBehaviour {

	[SerializeField] protected new BoxCollider collider;
	public float power = 5;
	public Controllable parent;
	[Tooltip("Which of the Controllable's 2 attacks this applies to.")]
	[Range(1, 2)] public int index;
	public float knockbackPower = 0;
	public float gracePeriod = 0.5f;

	private void Reset() {
		collider = GetComponent<BoxCollider>();
		collider.isTrigger = true;
	}

	protected void OnTriggerEnter(Collider other) {
		Controllable character = other.gameObject.GetComponent<Controllable>();
		if(character != null && character != parent && !character.invincible) {
			character.Damage(power, gracePeriod);
			if(knockbackPower > 0) {
				character.Knockback(knockbackPower * (character.transform.position - parent.transform.position).normalized);
			}
		}
	}

	protected void OnTriggerStay(Collider other) {
		OnTriggerEnter(other);
	}

	// Update power, in case it's changed for some reason.
	protected void OnEnable() {
		if(index == 1) {
			power = parent.attack1Power;
		} else {
			power = parent.attack2Power;
		}
	}
}
