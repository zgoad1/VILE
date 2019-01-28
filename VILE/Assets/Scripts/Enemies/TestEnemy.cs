using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : Enemy {

	public TMPro.TextMeshPro tmp;

	protected override void Reset() {
		base.Reset();
		tmp = GetComponent<TMPro.TextMeshPro>();
		gravity = 0;
	}

	protected override void AIUpdate() {
		base.AIUpdate();
		transform.LookAt(GameController.mainCam.transform);
		if(CanSeePlayer()) tmp.text = ">:(";
		else tmp.text = ":I";
	}

	public override void Stun() {
		base.Stun();
		tmp.text = "ouch";
	}
}
