using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionEvent : MonoBehaviour {

	[SerializeField] private EventQueue eq;

	protected void OnTriggerEnter(Collider other) {
		if(other.GetComponent<Controllable>() != null) {
			Instantiate(eq);
		}
	}
}
