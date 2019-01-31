using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcChalice : Enemy {
	[SerializeField] protected Transform middle;

	protected Quaternion newRot = Quaternion.identity;
	protected Vector3 newEuler = Vector3.zero;

	protected override void Reset() {
		base.Reset();
		middle = transform.Find("Middle");
	}

	protected override void AIUpdate() {
		base.AIUpdate();
		newEuler.y = distanceFromPlayer * 4;
		newRot.eulerAngles = newEuler;
		middle.rotation = newRot;
	}
}
