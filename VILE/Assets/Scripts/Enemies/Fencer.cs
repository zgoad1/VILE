using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fencer : Enemy {
	public Transform handL, handR;
	public Transform connector0, connector1;

	[HideInInspector] public Fencer partnerL, partnerR;

	private float handOffset = 3;
	private Vector3 iHandLPos, iHandRPos;
	private Quaternion iHandLRot, iHandRRot;
	private FencerState state = FencerState.WANDER;
	private float attackRadius = 30;

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
				state = FencerState.ATTACK;
		} else if(CanSeePlayer() || Vector3.Distance(transform.position, tracker.playerPosition) > 8) {
				state = FencerState.FOLLOW;
		} else {
			state = FencerState.WANDER;
		}

		Vector3 dist = Vector3.zero;	// this is what velocity lerps to

		switch(state) {
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
