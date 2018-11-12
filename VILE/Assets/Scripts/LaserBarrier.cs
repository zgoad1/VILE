using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBarrier : MonoBehaviour {

	[SerializeField] private List<MeshRenderer> children;
	private bool animate = false;

	private void Reset() {
		children = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());
		children.Remove(GetComponent<MeshRenderer>());
		if(GetComponent<BoxCollider>() == null) gameObject.AddComponent<BoxCollider>();
	}

	// Use this for initialization
	void Start () {
		Reset();
	}
	
	// Update is called once per frame
	void Update () {
		if(animate) {
			foreach(MeshRenderer r in children) {
				r.sharedMaterial.color = GameController.laserColor;//r.enabled = (GameController.frames / 2) % 2 == 1 ? true : false;
			}
		}
	}

	private void OnBecameInvisible() {
		animate = false; 
	}

	private void OnBecameVisible() {
		animate = true;
	}
}
