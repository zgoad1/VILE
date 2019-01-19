using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetableRenderer : MonoBehaviour {

	public Targetable parent;

	private void Reset() {
		parent = GetComponentInParent<Targetable>();
	}

	protected void OnBecameVisible() {
		Targetable.onScreen.Add(parent);
		parent.isOnScreen = true;
		//Debug.Log("visible! Controllable.onScreen.count: " + Controllable.onScreen.Count);
	}

	protected void OnBecameInvisible() {
		Targetable.onScreen.Remove(parent);
		parent.isOnScreen = false;
		Player.targets.Remove(parent);
		//Debug.Log("INvisible! Controllable.onScreen.count: " + Controllable.onScreen.Count);
	}

	// this is probably necessary
	protected virtual void OnDestroy() {
		OnBecameInvisible();
	}
}
