using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FlashEye : Controllable {

	private FlashEyeBall ball;
	private Transform player;
	private float maxVel = 80;
	private Vector3 myRot = Vector3.zero;

	protected override void Reset() {
		base.Reset();
		ball = GetComponentInChildren<FlashEyeBall>();
		player = FindObjectOfType<Player>().transform;
		rb.isKinematic = false;
	}
	
	// Update is called once per frame
	protected override void Update () {
		base.Update();
		if(control == state.AI) {
			transform.LookAt(player);
			Vector3 r2 = transform.rotation.eulerAngles;
			myRot.x = Mathf.Min(rb.velocity.magnitude / 2, 15);
			myRot.y = Mathf.Lerp(myRot.y, r2.y, 0.05f);
			transform.rotation = Quaternion.Euler(myRot);
		} else {
			// player stuff (basics taken care of in base.Update)
		}
	}

	private void FixedUpdate() {
		if(control == state.AI) {
			Vector3 dist = player.position - transform.position;
			dist.y = 0;
			rb.velocity = Vector3.ClampMagnitude(rb.velocity + dist.normalized, maxVel);
		}
	}
}
