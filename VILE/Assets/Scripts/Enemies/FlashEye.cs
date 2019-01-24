using System.Collections;
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
	private LayerMask solidLayer;
	private Vector3 desiredPosition = Vector3.zero; // desired position based on desiredDistance
	private float distanceFromDistance = 30;		// max distance from desiredDistance in which we may attack



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

	protected override void Update() {
		base.Update();

		transform.forward = Vector3.Slerp(transform.forward, velocity, 0.5f);

		// TODO: make it rotate about the main camera's x-axis (and z-axis for sideways?), corresponding to input
		// controls for more realistic rotation
		newEuler.x = Mathf.Min(velocityPerSec.magnitude / 2, 15);  // x rotation is dependent on speed
		newEuler.y = transform.eulerAngles.y;
		newEuler.z = transform.eulerAngles.z;
		transform.eulerAngles = newEuler;
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
			velocity = Vector3.Lerp(velocity, Vector3.zero, 0.01f);
			velocityPerSec = velocity * 60;
		}

		// Move
		//transform.position = desiredPosition;
		cc.Move(velocityPerSec * Time.smoothDeltaTime);
		RotateWithVelocity();
	}

	protected override void PlayerUpdate() {
		SetControls();
		if(!attacking) {
			SetMotion();    // velocity is set here
			//velocity = Vector3.Lerp(velocity, Vector3.zero, 0.1f * 60 * Time.smoothDeltaTime);

			if(atk1Key && CanAttack(atk1Cost)) Attack1();
			else if(atk2Key && CanAttack(atk2Cost)) Attack2();
		} else {
			velocity = Vector3.Lerp(velocity, Vector3.zero, 0.05f * 60 * Time.smoothDeltaTime);
			//RotateWithVelocity();
		}
		velocityPerSec = velocity * 60;

		//Debug.Log("Velocity: " + velocity);
		cc.Move(velocityPerSec * Time.smoothDeltaTime);
		SetTarget();

		EnemyPlayerUpdate();
	}

	protected override void SetMotion() {
		base.SetMotion();
		//velocity = Vector3.ClampMagnitude(velocity, maxVel / 60);
		// whoops that function didn't do what I thought it did

		if(velocity.magnitude > maxVel) {
			velocity /= (velocity.magnitude / maxVel);
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
			gameObject.layer = LayerMask.NameToLayer("Enemies");
			onGround = false;
		} else {
			// keep rotating blades if we're still in the air
			anim.speed = prevAnimSpeed;
		}
		laser.StopLaser();
		velocityPerSec = Vector3.zero;
		RotateWithVelocity();
	}

	private void RotateWithVelocity() {
		// i played with math until it worked
		if(target != null) {
			rotateTowards.x = Mathf.Lerp(rotateTowards.x, tracker.playerPosition.x, 0.1f);
			rotateTowards.y = transform.position.y;
			rotateTowards.z = Mathf.Lerp(rotateTowards.z, tracker.playerPosition.z, 0.1f);
		} else {
			rotateTowards = transform.position + transform.forward;
			rotateTowards.y = transform.position.y;
		}
		Quaternion r1 = transform.rotation;
		transform.LookAt(rotateTowards);
		Quaternion r2 = transform.rotation;
		Quaternion newRot = Quaternion.Slerp(r1, r2, 0.02f);
		newEuler.x = Mathf.Min(velocityPerSec.magnitude / 2, 15);  // x rotation is dependent on speed
		newEuler.y = newRot.eulerAngles.y;
		newEuler.z = newRot.eulerAngles.z;
		newRot.eulerAngles = newEuler;
		transform.rotation = newRot;
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
