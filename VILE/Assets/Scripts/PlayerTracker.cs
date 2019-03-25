using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTracker : MonoBehaviour {
	[HideInInspector] public Vector3 playerPosition;

	protected virtual void Start() {
		playerPosition = transform.position;
	}

	// Omniscient.
	protected virtual void Update() {
		playerPosition = GameController.player.transform.position;
	}
}
