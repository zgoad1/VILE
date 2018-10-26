using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashEyeBall : MonoBehaviour {

	private Player player;

	void Reset() {
		player = FindObjectOfType<Player>();
	}

	// Use this for initialization
	void Start () {
		Reset();
	}
	
	// Update is called once per frame
	void Update () {
		transform.forward = Vector3.Lerp(transform.forward, (player.transform.position - transform.position).normalized, 0.06f);
	}
}
