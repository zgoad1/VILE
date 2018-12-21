using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Controllable {
	
	protected static Player player;

	protected override void Reset() {
		base.Reset();

		if(renderer != null) {
			TargetableRenderer cr;
			cr = renderer.GetComponent<TargetableRenderer>();
			if(cr == null) {
				cr = renderer.gameObject.AddComponent<TargetableRenderer>();
			}
			cr.parent = this;
		} else {
			Debug.LogWarning("You have an Enemy with a weird renderer (not mesh)");
		}

		player = FindObjectOfType<Player>();
	}

	protected override void PlayerUpdate() {
		base.PlayerUpdate();
		target = GameController.player.target;
	}

	protected override void Update() {
		base.Update();
		distanceFromPlayer = Vector3.Distance(player.transform.position, transform.position);
	}

	protected override void Start() {
		base.Start();
		SetTarget();
	}

	protected override void SetTarget() {
		base.SetTarget();
		if(control == state.AI) target = FindObjectOfType<Player>();
		else target = player.target;
	}
}
