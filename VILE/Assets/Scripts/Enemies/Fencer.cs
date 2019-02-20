using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fencer : Enemy {
	public Transform handL, handR;
	public Transform connectorF0, connectorF1;
	public TransformBand connectorM0, connectorM1;
	public TessClaw[] fence;

	private float handOffset = 3;
	private Vector3 iHandLPos, iHandRPos;
	private Quaternion iHandLRot, iHandRRot;
	private FencerState fencerState = FencerState.WANDER;
	private float attackRadius = 30;
	private float ithickness;
	[HideInInspector] public List<Fencer> rightPartners = new List<Fencer>();
	[HideInInspector] public Fencer partner;
	private List<Fencer> leftPartners = new List<Fencer>();
	private Vector3 desiredPosition;

	private static int attackingPlayer = 0;		// amount of Fencers currently attacking the player

	enum FencerState {
		WANDER, FOLLOW, ATTACK
	}


	protected override void Reset() {
		base.Reset();
		TessClaw[] fence = GetComponentsInChildren<TessClaw>();
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
				ResetHandPositions();
				partner = null;
				break;

			case FencerState.FOLLOW:
				/* If we have a partner, try to maximize the length of our fence;
				 * else, move towards the player.
				 */
				ResetHandPositions();
				UpdatePartner();
				if(partner != null) {
					// move towards desired position - separate us from partner such that the player is in the middle (or not)
					desiredPosition = partner.transform.position - 10 * partner.transform.right;
				} else {
					desiredPosition = tracker.playerPosition;
				}
				newVelocity = desiredPosition - transform.position;
				newVelocity.y = 0;
				break;

			case FencerState.ATTACK:
				//SetHandPosition(handR, 1);
				//SetHandPosition(handL, -1);
				if(!attacking && CanAttack(atk1Cost)) {
					Attack1();
				}
				UpdatePartner();
				break;
		}

		#endregion

		velocity = Vector3.Lerp(velocity, newVelocity.normalized * speed, accel);
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
				float distSquared = (f.transform.position - transform.position).sqrMagnitude;
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
		base.Attack1();
		StartCoroutine("Attack1CR");
	}

	protected void StopAttack1() {
		StopCoroutine("Attack1CR");
		ResetHandPositions();
	}

	protected IEnumerator Attack1CR() {
		for(int i = 0; i < 125; i++) {
			float newPosition = -Mathf.Pow((i - 60) / 30, 2) + 5;
			yield return new WaitForSeconds(1f / 60f);
		}
		if(target is Controllable) ((Controllable)target).Damage(attack1Power);
		ResetHandPositions();
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
		// if we get errors, loop through rightPartners as well like above

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
	protected void SetHandPosition(Transform hand, int position) {
		hand.position = tracker.playerPosition + position * GameController.player.transform.right * handOffset;
		hand.right = hand.position - tracker.playerPosition;
	}

	/*
	 * Move hands back to resting positions
	 */
	protected void ResetHandPositions() {
		handL.localPosition = iHandLPos;
		handR.localPosition = iHandRPos;
		handL.localRotation = iHandLRot;
		handR.localRotation = iHandRRot;
	}
}
