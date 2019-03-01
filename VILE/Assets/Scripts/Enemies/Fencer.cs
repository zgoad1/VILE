using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//	TODO:
//	- Add fence damage
//	- Implement attacking while possessed
//	- Add a second attack

public class Fencer : Enemy {
	public Transform handL, handR;
	public Transform connectorF0, connectorF1;
	public TransformBand connectorM0, connectorM1;
	public TessClaw[] fence;
	public float handOffset;

	private Vector3 iHandLPos, iHandRPos;
	private Quaternion iHandLRot, iHandRRot;
	private FencerState fencerState = FencerState.WANDER;
	private float attackRadius = 30;
	private float ithickness;
	[HideInInspector] public List<Fencer> rightPartners = new List<Fencer>();
	[HideInInspector] public Fencer partner;
	private List<Fencer> leftPartners = new List<Fencer>();
	private Vector3 desiredPosition;
	private SkinnedMeshRenderer[] movingParts = new SkinnedMeshRenderer[4];

	enum FencerState {
		WANDER, FOLLOW, ATTACK
	}


	protected override void Reset() {
		base.Reset();
		fence = GetComponentsInChildren<TessClaw>();
	}

	protected override void Start() {
		base.Start();
		iHandLPos = handL.localPosition;
		iHandRPos = handR.localPosition;
		iHandLRot = handL.localRotation;
		iHandRRot = handR.localRotation;
		ithickness = fence[0].thicknessRandomness;
		foreach(TessClaw f in fence) {
			f.thicknessRandomness = 0;
			f.enabled = false;
		}
		movingParts[0] = transform.Find("Hand_L").GetComponent<SkinnedMeshRenderer>();
		movingParts[1] = transform.Find("Hand_R").GetComponent<SkinnedMeshRenderer>();
		movingParts[2] = transform.Find("Joint_L").GetComponent<SkinnedMeshRenderer>();
		movingParts[3] = transform.Find("Joint_R").GetComponent<SkinnedMeshRenderer>();
	}

	/**
	 * Use a state machine to determine actions.
	 * 
	 * WANDER - Player isn't in sight; wander aimlessly but stay in groups
	 * FOLLOW - Player can be seen but we're too far to attack; pursue them
	 * ATTACK - We are within arm's reach of the player; execute attack method if possible
	 */
	protected override void AIUpdate() {
		#region Set state

		// If we can see the player and we're close enough, attack
		if(CanSeePlayer() && distanceFromPlayerSquared < attackRadius * attackRadius) {
			fencerState = FencerState.ATTACK;

		// If we're not close enough to attack but we can see the player, or we're still not near
		// where we last saw the player, approach that spot
		} else if(CanSeePlayer() || Vector3.Distance(transform.position, tracker.playerPosition) > 8) {
			fencerState = FencerState.FOLLOW;

		// Else just wander
		} else {
			fencerState = FencerState.WANDER;
		}

		#endregion
		
		Vector3 newVelocity = Vector3.zero;

		#region State machine

		switch(fencerState) {
			case FencerState.WANDER:
				StopAttack1();  // calls ResetHandPositions
				partner = null;
				break;

			case FencerState.FOLLOW:
				/* If we have a partner, try to maximize the length of our fence;
				 * else, move towards the player.
				 */
				StopAttack1();	// calls ResetHandPositions
				UpdatePartner();
				if(partner != null) {
					// move towards desired position - separate us from partner such that the player is in the middle (or not)
					desiredPosition = partner.transform.position - 10 * partner.transform.right;
				} else {
					desiredPosition = tracker.playerPosition;
				}
				newVelocity = desiredPosition - transform.position;
				newVelocity.y = 0;
				RecalculateBounds(false);
				break;

			case FencerState.ATTACK:
				if(!attacking && CanAttack(atk1Cost)) {
					Attack1();
				} else if(attacking && !anim.GetBool("attack1")) {
					// move our hands back if we're done attacking and still cooling down
					ResetHandPositions();
				} else {
					// we're in the process of attacking
					SetHandPosition(handL, -handOffset);
					SetHandPosition(handR, handOffset);
					RecalculateBounds(true);
				}
				UpdatePartner();
				break;
		}

		#endregion

		velocity = Vector3.Lerp(velocity, newVelocity.normalized * speed, accel * 60 * Time.deltaTime);
		CommonUpdate();
	}

	/*
	 * Add ourselves to other Fencers' partner lists when we enter their trigger area
	 */
	protected void OnTriggerEnter(Collider other) {
		FencerPartnerFinder ff = other.gameObject.GetComponent<FencerPartnerFinder>();
		if(ff != null && ff.parent != this) {
			ff.parent.rightPartners.Add(this);
			leftPartners.Add(ff.parent);
		}
	}

	/*
	 * Remove ~ when we exit
	 */
	protected void OnTriggerExit(Collider other) {
		FencerPartnerFinder ff = other.gameObject.GetComponent<FencerPartnerFinder>();
		if(ff != null) {
			ff.parent.rightPartners.Remove(this);
			leftPartners.Remove(ff.parent);
		}
	}

	//private void OnDrawGizmos() {
	//	Gizmos.DrawSphere(partner != null ? partner.transform.position : Vector3.zero, 3);
	//}

	/* 
	 * Loop through 'partners' list to find nearest partner within a 90-degree angle
	 */
	protected void UpdatePartner() {
		partner = null;
		if(rightPartners.Count > 0) {
			float nearestDistSquared = Mathf.Infinity;
			foreach(Fencer f in rightPartners) {
				float distSquared = (f.transform.position - transform.position).sqrMagnitude;	// f may reference a destroyed object, which should be impossible
				float angle = Helper.AngleBetween(transform, f.transform);
				if(distSquared < nearestDistSquared && angle < 225 && angle > 135) {
					partner = f;
					nearestDistSquared = distSquared;
				}
			}
		}
		if(partner != null) {
			// connect fence ends to partner
			connectorM0.transform.SetParent(partner.connectorF0.transform);
			connectorM0.transform.localPosition = Vector3.zero;
			connectorM1.transform.SetParent(partner.connectorF1.transform);
			connectorM1.transform.localPosition = Vector3.zero;
			// increase fence thickness
			foreach(TessClaw f in fence) {
				f.thicknessRandomness = Mathf.Clamp(f.thicknessRandomness + 0.05f, 0, ithickness);
				f.enabled = true;
			}
		} else {
			// decrease fence thickness
			foreach(TessClaw f in fence) {
				f.thicknessRandomness = Mathf.Clamp(f.thicknessRandomness - 0.05f, 0, ithickness);
				if(f.thicknessRandomness == 0) f.enabled = false;
			}
		}
	}

	/*
	 * Double damage if player attacks from behind
	 */
	public override void DeductHP(float damage) {
		if(!Helper.IsInFrontOf(transform, GameController.player.transform)) {
			hp -= damage * 2;
		} else {
			hp -= damage;
		}
	}

	/*
	 * Recall hands when possessed
	 */
	public override void SetPlayer() {
		base.SetPlayer();
		ResetHandPositions();
	}

	/*
	 * Start a coroutine for the attack animation
	 */
	protected override void Attack1() {
		if(target != null) {
			base.Attack1();
			anim.SetBool("attack1", true);
			//StartCoroutine("Attack1CR");
		}
	}

	protected void StopAttack1() {
		if(anim.GetBool("attack1")) {
			anim.SetBool("attack1", false);
			//StopCoroutine("Attack1CR");
			cooldownTimer = 0;
			stamina += atk1Cost;
		}
		ResetHandPositions();
	}

	/*
	 * For some extremely nebulous reason, only when called in an animation event,
	 * Knockback does not permanently set the target's forward
	 */
	public void Attack1Damage() {
		if(target is Controllable) {
			((Controllable)target).Damage(attack1Power);
			((Controllable)target).AnimEventKnockback((target.transform.position - transform.position).normalized * 2);
		}
	}

	public void OnAttack1Finish() {
		anim.SetBool("attack1", false);
	}

	public override void Stun() {
		base.Stun();
		StopAttack1();
	}

	public override void Knockback(Vector3 force) {
		base.Knockback(force / 4f);	// we're really slidey so decrease the knockback a bit
	}

	/* 
	 * Take care of cleanup before we are destroyed; make sure nothing is still
	 * trying to access us
	 */
	protected override void Die() {
		// Remove this fencer from any partner lists it's in
		foreach(Fencer f in leftPartners) {
			f.rightPartners.Remove(this);
		}
		foreach(Fencer f in rightPartners) {
			// This may or may not be necessary.
			// If a Fencer is seen with no rightPartner but has visible fence, remove it
			f.leftPartners.Remove(this);
		}

		// Un-parent any male connectors from our female connectors so they're not destroyed with us
		List<Transform> toUnparent = new List<Transform>();
		foreach(Transform t in connectorF0) {
			if(t.gameObject.name.Contains("Fence")) {
				toUnparent.Add(t);
			}
		}
		foreach(Transform t in connectorF1) {
			if(t.gameObject.name.Contains("Fence")) {
				toUnparent.Add(t);
			}
		}
		foreach(Transform t in toUnparent) {
			t.SetParent(null);
		}

		base.Die();
	}

	/*
	 * Move a hand to a specified position relative to the player
	 */
	protected void SetHandPosition(Transform hand, float position) {
		hand.position = Vector3.Lerp(
			hand.position, 
			tracker.playerPosition + position * transform.right,
			0.2f
		);
		hand.right = Vector3.Slerp(
			hand.right,
			hand.position - tracker.playerPosition,
			0.2f
		);
	}

	/*
	 * Move hands back to resting positions
	 */
	protected void ResetHandPositions() {
		handL.localPosition = Vector3.Lerp(handL.localPosition, iHandLPos, 0.1f);
		handR.localPosition = Vector3.Lerp(handR.localPosition, iHandRPos, 0.1f);
		handL.localRotation = Quaternion.Slerp(handL.localRotation, iHandLRot, 0.1f);
		handR.localRotation = Quaternion.Slerp(handR.localRotation, iHandRRot, 0.1f);
	}

	/*
	 * These SkinnedMeshRenderers do not update the bounds on their own at all, so
	 * we call this method to enable updating when offscreen when the fencer is
	 * close enough
	 */
	protected void RecalculateBounds(bool b) {
		foreach(SkinnedMeshRenderer m in movingParts) {
			m.updateWhenOffscreen = b;
		}
	}
}
