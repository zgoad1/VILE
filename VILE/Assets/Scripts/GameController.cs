using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	public static Player player;
	public static int frames = 0;

	// laser barriers
	[SerializeField] private Material laserBarrier;
	private Color newColorLB;
	private float initialAlphaLB;

	// wall glow animation
	[SerializeField] private Material wallGlow;
	private Color newColorWG = Color.black;

	public void FindPlayer() {
		player = FindObjectOfType<Player>();
	}

	private void Reset() {
		FindPlayer();
		frames = 0;
		newColorLB = laserBarrier.color;
		initialAlphaLB = newColorLB.a;
	}

	// Use this for initialization
	void Awake () {
		Reset();
	}
	
	// Update is called once per frame
	void Update () {
		newColorWG.r = 0.5f * Mathf.Sin(2 * Mathf.PI / 4 * Time.time) + 0.5f;
		wallGlow.SetColor("_EmissionColor", newColorWG);

		newColorLB.a = 0.1f * Mathf.Sin(2 * Mathf.PI / 0.12f * Time.time) + .9f;
		laserBarrier.color = newColorLB;

		frames++;
	}

	private void OnDestroy() {
		newColorWG.r = 0;
		wallGlow.SetColor("_EmissionColor", newColorWG);

		newColorLB.a = initialAlphaLB;
		laserBarrier.color = newColorLB;
	}
}
