using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Controllable {

	[HideInInspector] public bool stunned = false;

	protected override void Start() {
		base.Start();
		SetTarget();
	}

	protected override void SetTarget() {
		base.SetTarget();
		target = FindObjectOfType<Player>();
	}
}
