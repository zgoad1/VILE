using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashEye : Enemy {

	private FlashEyeBall ball;
	private float maxVel = 50;
	private Vector3 newEuler = Vector3.zero;
	private Vector3 targetPos = Vector3.zero;
	private Vector3 velocityPerSec = Vector3.zero;
	private LaserFX laser;
	private LayerMask solidLayer;

	protected override void Reset() {
		base.Reset();
		ball = GetComponentInChildren<FlashEyeBall>();
		laser = GetComponentInChildren<LaserFX>();
		solidLayer = LayerMask.NameToLayer("Solid");
		rb.isKinematic = false;
		camDistance = 25;
	}

	protected override void OnControllerColliderHit(ControllerColliderHit hit) {
		// if we hit the ground relatively quickly
		if(!onGround && hit.gameObject.layer == solidLayer && velocity.y < -0.5f) {
			// shake strength inversely proportional to distance from player
			GameController.camControl.ScreenShake(1f - distanceFromPlayer / 150f);
		}
		base.OnControllerColliderHit(hit);	// sets onGround
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
		if(!attacking) {
			SetMotion();    // velocity is set here
			velocityPerSec = velocity * 60;

			transform.forward = Vector3.Slerp(transform.forward, velocity, 0.05f);
			// TODO: make it rotate about the main camera's x-axis (and z-axis for sideways?), corresponding to input
			// controls for more realistic rotation
			newEuler.x = Mathf.Min(velocityPerSec.magnitude / 2, 15);  // x rotation is dependent on speed
			newEuler.y = transform.eulerAngles.y;
			newEuler.z = transform.eulerAngles.z;
			transform.eulerAngles = newEuler;

			if(atk1Key && CanAttack(atk1Cost)) Attack1();
			else if(atk2Key && CanAttack(atk2Cost)) Attack2();
		} else {
			velocity = Vector3.Lerp(velocity, Vector3.zero, 0.02f * 60 * Time.smoothDeltaTime);
			RotateWithVelocity();
		}
		cc.Move(velocity);
		SetTarget();
	}

	public override void Stun() {
		prevAnimSpeed = anim.speed;
		base.Stun();
		if(stunCount >= 2) {
			maxVel = 10;
			// make flying enemies fall
			gameObject.layer = LayerMask.NameToLayer("Characters");
			onGround = false;
		} else {
			// keep rotating blades if we're still in the air
			anim.speed = prevAnimSpeed;
		}
		velocityPerSec = Vector3.zero;
		RotateWithVelocity();
	}

	private void RotateWithVelocity() {
		// i played with math until it worked
		if(target != null) {
			targetPos.x = Mathf.Lerp(targetPos.x, target.transform.position.x, 0.1f);
			targetPos.y = transform.position.y;
			targetPos.z = Mathf.Lerp(targetPos.z, target.transform.position.z, 0.1f);
		} else {
			targetPos = transform.position + transform.forward;
			targetPos.y = transform.position.y;
		}
		Quaternion r1 = transform.rotation;
		transform.LookAt(targetPos);
		Quaternion r2 = transform.rotation;
		Quaternion newRot = Quaternion.Slerp(r1, r2, 0.02f);
		newEuler.x = Mathf.Min(velocityPerSec.magnitude / 2, 15);  // x rotation is dependent on speed
		newEuler.y = newRot.eulerAngles.y;
		newEuler.z = newRot.eulerAngles.z;
		newRot.eulerAngles = newEuler;
		transform.rotation = newRot;
	}

	protected override void Attack2() {
		base.Attack2();
		laser.ShootLaser();
	}
}
