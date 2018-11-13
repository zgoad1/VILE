using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Controllable {

	[HideInInspector] public bool stunned = false;
	protected static Player player;

	[HideInInspector] public bool isTarget = false;
	[HideInInspector] public float distanceFromPlayer = 0;
	[HideInInspector] public static List<Enemy> onScreen = new List<Enemy>();
	[HideInInspector] public Vector3 screenCoords = Vector3.zero;

	protected override void Reset() {
		base.Reset();
		player = FindObjectOfType<Player>();
	}

	protected override void PlayerUpdate() {
		base.PlayerUpdate();
		target = GameController.player.target;
	}

	protected override void AIUpdate() {
		base.AIUpdate();
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

	public virtual void SetScreenCoords() {
		screenCoords = mainCam.WorldToScreenPoint(camLook.position);
		screenCoords.x /= Screen.width;
		screenCoords.y /= Screen.height;
	}
}
