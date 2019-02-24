using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**For setting animation parameters for landing animations a bit earlier than
 * when we hit the ground.
 */

[RequireComponent(typeof(Collider))]
public class GroundTest : MonoBehaviour {

	public LayerMask groundLayers = 512;
	protected Controllable parent;
	protected List<Collider> grounds = new List<Collider>();
	public bool onGround = false;
	private bool aboutToLeave = false;

	private void Reset() {
		parent = GetComponentInParent<Controllable>();
		GetComponent<Collider>().isTrigger = true;
	}

	public void PublicReset() {
		onGround = false;
		Leave();
		grounds.Clear();
	}

	private void Start() {
		Reset();
	}

	private void OnTriggerEnter(Collider other) {
		if(LayerIsGround(other.gameObject.layer)) {
			grounds.Add(other);
			if(grounds.Count == 1 && !onGround) {
				if(aboutToLeave) {
					StopCoroutine("WaitToLeave");
					aboutToLeave = false;
				} else {
					Land();
				}
			} else {
				//Debug.Log("grounds: " + grounds.Count + "\ny-velocity: " + parent.velocity.y);
			}
		}
	}

	private void OnTriggerStay(Collider other) {
		if(LayerIsGround(other.gameObject.layer)) {
			onGround = true;
		}
	}

	private void OnTriggerExit(Collider other) {
		if(LayerIsGround(other.gameObject.layer)) {
			grounds.Remove(other);
			if(grounds.Count == 0) {
				if(onGround) {
					StartCoroutine("WaitToLeave");
					//Leave();
				}
				onGround = false;
			}
		}
	}

	private bool LayerIsGround(int layer) {
		return groundLayers == (groundLayers | (1 << layer));
	}

	protected virtual void Land() {
		parent.onGround = true;
		parent.yMove.y = 0;
	}
	protected virtual void Leave() {
		parent.onGround = false;
	}

	/* Wait a bit to avoid triggering extra land animations, which can break things
	 */
	private IEnumerator WaitToLeave() {
		aboutToLeave = true;
		yield return new WaitForSeconds(0.1f);
		Leave();
		aboutToLeave = false;
	}
}
