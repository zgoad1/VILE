using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventItem : MonoBehaviour {

	protected EventQueue eq;

	// Use this for initialization
	protected virtual void Start () {
		Reset();
	}

	protected virtual void Reset() {
		eq = FindObjectOfType<EventQueue>();
	}
}
