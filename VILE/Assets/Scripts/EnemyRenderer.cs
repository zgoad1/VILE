using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRenderer : ControllableRenderer {

	protected override void OnBecameVisible() {
		Enemy.onScreen.Add((Enemy)parent);
		//Debug.Log("visible! Controllable.onScreen.count: " + Controllable.onScreen.Count);
	}

	protected override void OnBecameInvisible() {
		Enemy.onScreen.Remove((Enemy)parent);
		//Debug.Log("INvisible! Controllable.onScreen.count: " + Controllable.onScreen.Count);
	}

	// this is probably necessary
	protected virtual void OnDestroy() {
		OnBecameInvisible();
	}
}
