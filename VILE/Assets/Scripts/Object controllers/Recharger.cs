using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TessClaw))]
public class Recharger : MonoBehaviour {

	public float radius = 25;
	public float thicknessMultiplier = 1;
	
	private TransformChain leaf;
	private TessClaw vfx;

	private void Start() {
		vfx = GetComponent<TessClaw>();
		leaf = GetComponentInChildren<TransformChain>();
		while(leaf.next.Length > 0) {
			leaf = leaf.next[0];
		}
	}

	private void Update() {
		float dist = Vector3.Distance(transform.position, GameController.player.transform.position);
		if(dist < radius) {
			vfx.enabled = true;
			vfx.thicknessRandomness = thicknessMultiplier * (radius - dist) / radius;
			leaf.transform.position = GameController.playerTarget.position;
			GiveStamina(1);
		} else {
			vfx.enabled = false;
		}
	}

	protected virtual void GiveStamina(float st) {
		GameController.player.stamina += st * Time.deltaTime * 60;
	}
}
