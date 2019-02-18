using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fencer : Enemy {
	public Transform handL, handR;
	public Transform connectorF0, connectorF1;
	public TransformBand connectorM0, connectorM1;

	[HideInInspector] public Fencer partnerL, partnerR;

	private float handOffset = 3;
	private Vector3 iHandLPos, iHandRPos;
	private Quaternion iHandLRot, iHandRRot;
	private FencerState fencerState = FencerState.WANDER;
	private float attackRadius = 30;
	public Fencer partner;
	[HideInInspector] public List<Fencer> partners = new List<Fencer>();

	enum FencerState {
		WANDER, FOLLOW, ATTACK
	}

	protected override void Start() {
		base.Start();
		iHandLPos = handL.localPosition;
		iHandRPos = handR.localPosition;
		iHandLRot = handL.localRotation;
		iHandRRot = handR.localRotation;
	}

	protected override void AIUpdate() {
		if(CanSeePlayer() && distanceFromPlayerSquared < attackRadius * attackRadius) {
			fencerState = FencerState.ATTACK;
			UpdatePartner();
		} else if(CanSeePlayer() || Vector3.Distance(transform.position, tracker.playerPosition) > 8) {
			fencerState = FencerState.FOLLOW;
			UpdatePartner();
		} else {
			fencerState = FencerState.WANDER;
		}

		Vector3 dist = Vector3.zero;	// this is what velocity lerps to

		switch(fencerState) {
			case FencerState.WANDER:
				ResetHandPositions();
				break;

			case FencerState.FOLLOW:
				ResetHandPositions();
				dist = tracker.playerPosition - transform.position;
				dist.y = 0;
				break;

			case FencerState.ATTACK:
				SetHandPosition(handR, 1);
				SetHandPosition(handL, -1);
				break;
		}
		
		velocity = Vector3.Lerp(velocity, dist.normalized * speed, accel);
		PlayerMove();
	}

	protected void OnTriggerEnter(Collider other) {
		FencerPartnerFinder ff = other.gameObject.GetComponent<FencerPartnerFinder>();
		if(ff != null) {
			ff.parent.partners.Add(this);
		}
	}

	protected void OnTriggerExit(Collider other) {
		FencerPartnerFinder ff = other.gameObject.GetComponent<FencerPartnerFinder>();
		if(ff != null) {
			ff.parent.partners.Remove(this);
		}
	}

	private void OnDrawGizmos() {
		Gizmos.DrawSphere(partner != null ? partner.transform.position : Vector3.zero, 3);
	}

	// loop through 'partners' list to find nearest partner within a 45-degree angle
	protected void UpdatePartner() {
		partner = null;
		if(partners.Count > 0) {
			float nearestDistSquared = Mathf.Infinity;
			foreach(Fencer f in partners) {
				float distSquared = (f.transform.position - transform.position).sqrMagnitude;
				float angle = Helper.AngleBetween(transform, f.transform);
				if(distSquared < nearestDistSquared && angle < 202.5 && angle > 157.5) {
					partner = f;
					nearestDistSquared = distSquared;
				}
			}
		}
		if(partner != null) {
			connectorM0.transform.SetParent(partner.connectorF0.transform);
			connectorM0.transform.localPosition = Vector3.zero;
			connectorM1.transform.SetParent(partner.connectorF1.transform);
			connectorM1.transform.localPosition = Vector3.zero;
		}
	}

	// double damage if player attacks from behind
	public override void DeductHP(float damage) {
		if(!Helper.IsInFrontOf(transform, GameController.player.transform)) {
			hp -= damage * 2;
		} else {
			hp -= damage;
		}
	}

	public override void SetPlayer() {
		base.SetPlayer();
		ResetHandPositions();

	}

	protected void SetHandPosition(Transform hand, int direction) {
		hand.transform.position = tracker.playerPosition + direction * GameController.player.transform.right * handOffset;
		hand.transform.right = hand.transform.position - tracker.playerPosition;
	}

	protected void ResetHandPositions() {
		handL.localPosition = iHandLPos;
		handR.localPosition = iHandRPos;
		handL.localRotation = iHandLRot;
		handR.localRotation = iHandRRot;
	}
}
