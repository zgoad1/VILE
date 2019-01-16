using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTracker : MonoBehaviour {
	public Vector3 playerPosition;

	// Omniscient.
	void Update() {
		playerPosition = GameController.player.transform.position;
	}
}
