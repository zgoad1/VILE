using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FlashEye : Enemy {

	private FlashEyeBall ball;
	private float maxVel = 50;
	private Vector3 newEuler = Vector3.zero;
	private Vector3 playerPos = Vector3.zero;
	private Vector3 velocityPerSec = Vector3.zero;

	protected override void Reset() {
		base.Reset();
		ball = GetComponentInChildren<FlashEyeBall>();
		rb.isKinematic = false;
		camDistance = 25;
	}

	protected override void AIUpdate() {
		base.AIUpdate();
		// set velocity
		Vector3 dist = target.transform.position - transform.position;
		dist.y = 0;
		velocityPerSec = Vector3.ClampMagnitude(velocityPerSec + dist.normalized, maxVel);  // from per second to per frame
		velocity = velocityPerSec / 60;
		cc.Move(velocityPerSec * Time.smoothDeltaTime);
		RotateWithVelocity();
	}

	protected override void PlayerUpdate() {
		SetControls();
		SetMotion();	// velocity is set here
		velocityPerSec = velocity * 60;
		cc.Move(velocity);

		transform.forward = Vector3.Slerp(transform.forward, velocity, 0.05f);
		// TODO: make it rotate about the main camera's x-axis (and z-axis for sideways?), corresponding to input
		// controls for more realistic rotation
		newEuler.x = Mathf.Min(velocityPerSec.magnitude / 2, 15);  // x rotation is dependent on speed
		newEuler.y = transform.eulerAngles.y;
		newEuler.z = transform.eulerAngles.z;
		transform.eulerAngles = newEuler;

		SetTarget();
	}

	public override void Stun() {
		base.Stun();
		if(stunCount >= 2) {
			maxVel = 10;
		} else {
			// keep rotating blades if we're still in the air
			anim.speed = prevAnimSpeed;
		}
		velocity = Vector3.zero;
		velocityPerSec = Vector3.zero;
		cc.velocity.Set(0, 0, 0);
	}

	private void RotateWithVelocity() {
		// i played with math until it worked
		playerPos.x = Mathf.Lerp(playerPos.x, player.transform.position.x, 0.1f);
		playerPos.y = transform.position.y;
		playerPos.z = Mathf.Lerp(playerPos.z, player.transform.position.z, 0.1f);
		Quaternion r1 = transform.rotation;
		transform.LookAt(playerPos);
		Quaternion r2 = transform.rotation;
		Quaternion newRot = Quaternion.Slerp(r1, r2, 0.02f);
		newEuler.x = Mathf.Min(velocityPerSec.magnitude / 2, 15);  // x rotation is dependent on speed
		newEuler.y = newRot.eulerAngles.y;
		newEuler.z = newRot.eulerAngles.z;
		newRot.eulerAngles = newEuler;
		transform.rotation = newRot;
	}
}
