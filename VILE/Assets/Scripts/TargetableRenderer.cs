using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetableRenderer : MonoBehaviour {

	public Targetable parent;



	private void Reset() {
		parent = GetComponentInParent<Targetable>();
	}

	protected void Awake() {
		if(parent == null) {
			parent = GetComponentInParent<Targetable>();
		}
	}

	protected void OnBecameVisible() {
		if(parent.enabled) {
			Targetable.onScreen.Add(parent);
			parent.isOnScreen = true;
		}
		//Debug.Log(parent + " became visible");
		//Debug.Log("visible! Controllable.onScreen.count: " + Controllable.onScreen.Count);
	}

	protected void OnBecameInvisible() {
		if(parent.enabled) {
			Targetable.onScreen.Remove(parent);
			parent.isOnScreen = false;
			Player.targets.Remove(parent);
		}
		//Debug.Log(parent + " disappeared");
		//Debug.Log("INvisible! Controllable.onScreen.count: " + Controllable.onScreen.Count);
	}

	// this is probably necessary
	protected virtual void OnDestroy() {
		OnBecameInvisible();
	}
}
