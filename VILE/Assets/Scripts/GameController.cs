using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	public static Player player;
	public static int frames = 0;
	public static Color laserColor = Color.red;

	private void Reset() {
		player = FindObjectOfType<Player>();
		frames = 0;
	}

	// Use this for initialization
	void Start () {
		Reset();
	}
	
	// Update is called once per frame
	void Update () {
		laserColor.a = 0.1f * Mathf.Sin(2 * Mathf.PI / 0.12f * Time.time) + .9f;
		frames++;
	}
}
