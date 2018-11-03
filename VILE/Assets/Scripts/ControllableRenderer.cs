using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllableRenderer : MonoBehaviour {

	public Controllable parent;

	protected virtual void OnBecameVisible() {
	}

	protected virtual void OnBecameInvisible() {
	}
}
