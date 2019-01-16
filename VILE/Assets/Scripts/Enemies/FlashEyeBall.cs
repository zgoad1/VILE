using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashEyeBall : MonoBehaviour {

	private Player player;
	private FlashEye parent;
	private LayerMask layerMask;

	void Reset() {
		player = FindObjectOfType<Player>();
		// ball -> armature -> flash eye
		parent = GetComponentInParent<Transform>().GetComponentInParent<FlashEye>();
		layerMask = 1 << LayerMask.NameToLayer("Solid");
	}

	// Use this for initialization
	void Start () {
		Reset();
	}
	
	// Update is called once per frame
	void Update () {
		if(parent.control == Controllable.state.PLAYER && parent.target == null) {
			// if no target, raycast in the direction of the reticle and face the hit point
			// if that hits nothing, face in that direction
			RaycastHit hit;
			if(Physics.Raycast(Controllable.mainCam.transform.position, Controllable.mainCam.transform.forward, out hit, parent.sightLength, layerMask)) {
				transform.forward = Vector3.Slerp(transform.forward, (hit.point - transform.position).normalized, 0.5f * 60 * Time.deltaTime);
			} else {
				transform.forward = Vector3.Slerp(transform.forward, Controllable.mainCam.transform.forward, 0.2f * 60 * Time.deltaTime);
			}
		} else {
			// raise accuracy for player
			if(parent.control == Controllable.state.AI) {
				float accuracy = 0.3f;
				transform.forward = Vector3.Slerp(transform.forward, (GameController.playerTarget.position - transform.position).normalized, accuracy * 60 * Time.deltaTime);
			} else {
				float accuracy = 0.5f;
				transform.forward = Vector3.Slerp(transform.forward, (parent.target.camLook.transform.position - transform.position).normalized, accuracy * 60 * Time.deltaTime);
			}
		}
	}
}
