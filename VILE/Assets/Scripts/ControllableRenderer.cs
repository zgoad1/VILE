using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllableRenderer : MonoBehaviour {

	public Controllable parent;

	protected virtual void OnBecameVisible() {
		Controllable.onScreen.Add(parent);
		//Debug.Log("visible! Controllable.onScreen.count: " + Controllable.onScreen.Count);
	}

	protected virtual void OnBecameInvisible() {
		Controllable.onScreen.Remove(parent);
		//Debug.Log("INvisible! Controllable.onScreen.count: " + Controllable.onScreen.Count);
	}

	// this is probably necessary
	protected virtual void OnDestroy() {
		OnBecameInvisible();
	}
}
