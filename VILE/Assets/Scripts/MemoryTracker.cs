using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryTracker : PlayerTracker {
	/// <summary>
	/// Only update the player's perceived position when we can see them
	/// </summary>

	public bool playerVisible = false;
	public Enemy controller;

	private void Reset() {
		controller = GetComponent<Enemy>();
	}

	protected override void Update() {
		if(controller.CanSeePlayer()) {
			playerPosition = GameController.player.transform.position;
			playerVisible = true;
		} else {
			playerVisible = false;
		}
	}
}
