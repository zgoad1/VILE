using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentCuller : MonoBehaviour {

	[SerializeField] private List<MonoBehaviour> components = new List<MonoBehaviour>();

	private void Reset() {
		components.Add(GetComponent<LaserBarrier>());
	}

	private void OnBecameInvisible() {
		foreach(MonoBehaviour c in components) {
			c.enabled = false;
		}
	}

	private void OnBecameVisible() {
		foreach(MonoBehaviour c in components) {
			c.enabled = true;
		}
	}
}
