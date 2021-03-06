﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashEye : Enemy {

	[Tooltip("Enemy will try to maintain this distance from the player at all times.")]
	public float desiredDistance = 100;

	private FlashEyeBall ball;
	private float maxVel = 2.3f;
	private float maxAIVelPerSec = 64;
	private Vector3 newEuler = Vector3.zero;
	private Vector3 moveTowards = Vector3.zero;
	private Vector3 rotateTowards = Vector3.zero;
	private Vector3 velocityPerSec = Vector3.zero;
	private LaserFX laser;
	private Vector3 desiredPosition = Vector3.zero; // desired position based on desiredDistance
	private float distanceFromDistance = 30;		// max distance from desiredDistance in which we may attack



	protected override void Reset() {
		base.Reset();
		ball = GetComponentInChildren<FlashEyeBall>();
		laser = GetComponentInChildren<LaserFX>();
		camDistance = 25;
	}

	protected override void Update() {
		base.Update();

		transform.forward = Vector3.Slerp(transform.forward, velocity, 0.5f * 60 * Time.deltaTime);   // done in PlayerMove()
		RotateWithVelocity();
	}

	protected override void AIUpdate() {
		base.AIUpdate();

		if(!attacking) {
			//get variables
			moveTowards = tracker.playerPosition;
			moveTowards.y = transform.position.y;
			// If we can see the player, move to a safe distance from them; else, approach the point at which they were last seen.
			if(((MemoryTracker)tracker).playerVisible) {
				desiredPosition = moveTowards + desiredDistance * (transform.position - moveTowards).normalized;
			} else {
				desiredPosition = moveTowards;
			}
			Vector3 dist = desiredPosition - transform.position;
			dist.y = 0;

			// set velocity
			velocityPerSec += dist.normalized * 0.7f;
			if(velocityPerSec.magnitude > maxAIVelPerSec) {
				velocityPerSec /= (velocityPerSec.magnitude / maxAIVelPerSec);
			}
			velocity = velocityPerSec / 60;

			// attempt to attack if we're close enough to the desired position
			if(CanAttack(atk1Cost) && CanSeePlayer() && Mathf.Abs(distanceFromPlayer - desiredDistance) < distanceFromDistance) {
				Attack1();
			}
		} else {
			velocity = Vector3.Lerp(velocity, Vector3.zero, accel * 60 * Time.deltaTime);
			velocityPerSec = velocity * 60;
		}

		// Move
		ApplyGravity();
		cc.Move((velocityPerSec + yMove * 60) * Time.smoothDeltaTime);
	}

	protected override void PlayerUpdate() {
		base.PlayerUpdate();
		velocityPerSec = velocity * 60;
		EnemyPlayerUpdate();
	}

	protected override void SetVelocity() {
		base.SetVelocity();


		// Limit velocity
		if(velocity.sqrMagnitude > maxVel * maxVel) {
			velocity /= (velocity.sqrMagnitude / (maxVel * maxVel));
		}
	}

	public override void Stun() {
		prevAnimSpeed = anim.speed;
		base.Stun();
		if(stunCount == 2) {
			maxVel /= 4;
			maxAIVelPerSec /= 4;
			desiredDistance /= 4;
			distanceFromDistance /= 4;
			// make flying enemies fall
			Helper.SetAllLayers(gameObject, LayerMask.NameToLayer("Enemies"));
			GetComponentInChildren<GroundTest>().groundLayers = 1 << LayerMask.NameToLayer("Solid");
			GetComponentInChildren<GroundTest>().PublicReset();
		} else {
			// keep rotating blades if we're still in the air
			anim.speed = prevAnimSpeed;
		}
		laser.StopLaser();
		velocityPerSec = Vector3.zero;
		RotateWithVelocity();
	}

	private void RotateWithVelocity() {
		// TODO: make it rotate about the main camera's x-axis (and z-axis for sideways?), corresponding to input
		// controls for more realistic rotation
		newEuler.x = Mathf.Min(velocityPerSec.magnitude / 2, 15);  // x rotation is dependent on speed
		newEuler.y = transform.eulerAngles.y;
		newEuler.z = transform.eulerAngles.z;
		transform.eulerAngles = newEuler;
	}

	protected override void Attack1() {
		base.Attack1();
		laser.ShootLaser();
	}

	protected override void Attack2() {
		base.Attack2();
		laser.ShootLaser();
	}

	public override void Knockback(Vector3 force) {
		base.Knockback(force);
		laser.StopLaser();
	}
}
