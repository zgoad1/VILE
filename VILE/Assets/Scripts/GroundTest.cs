using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**For setting animation parameters for landing animations a bit earlier than
 * when we hit the ground.
 */

public class GroundTest : MonoBehaviour {

	private LayerMask ground;
	private Controllable parent;
	private List<Collider> grounds = new List<Collider>();

	private void Reset() {
		ground = LayerMask.NameToLayer("Solid");
		parent = GetComponentInParent<Controllable>();
	}

	private void Start() {
		Reset();
	}

	private void OnTriggerEnter(Collider other) {
		if(other.gameObject.layer == ground) {
			grounds.Add(other);
			if(grounds.Count == 1 && !parent.anim.GetBool("onGround")) {
				//Debug.Log("landing");
				parent.anim.SetTrigger("land");
			} else {
				//Debug.Log("grounds: " + grounds.Count + "\ny-velocity: " + parent.velocity.y);
			}
		}
	}

	private void OnTriggerStay(Collider other) {
		if(other.gameObject.layer == ground) {
			parent.anim.SetBool("onGround", true);
		}
	}

	private void OnTriggerExit(Collider other) {
		if(other.gameObject.layer == ground) {
			grounds.Remove(other);
			if(grounds.Count == 0) {
				if(parent.anim.GetBool("onGround")) {
					// set this trigger on the frame when we leave the ground
					parent.anim.SetTrigger("offGround");
				}
				parent.anim.SetBool("onGround", false);
			}
		}
	}
}
