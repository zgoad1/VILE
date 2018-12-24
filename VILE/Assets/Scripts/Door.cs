using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Targetable {
	public bool open = false;

	private Animator anim;
	private new MeshRenderer renderer;
	private ParticleSystem sparks;

	// Start is called before the first frame update
	protected virtual void Start() {
		Reset();
		renderer = GetComponentInChildren<MeshRenderer>();
		sparks = GetComponentInChildren<ParticleSystem>();

		if(renderer != null) {
			TargetableRenderer cr;
			cr = renderer.GetComponent<TargetableRenderer>();
			if(cr == null) {
				cr = renderer.gameObject.AddComponent<TargetableRenderer>();
			}
			cr.parent = this;
		} else {
			Debug.LogWarning("You have a Door with a weird renderer (not mesh)");
		}
		
		anim = GetComponent<Animator>();
		anim.CrossFade(open ? "Open" : "Close", 0);
		Open(open);
	}

	private void Update() {
		distanceFromPlayer = Vector3.Distance(GameController.player.transform.position, transform.position);
	}

	public void Open(bool newOpen) {
		canTarget = !newOpen;
		open = newOpen;
		anim.SetBool("Open", newOpen);
	}

	public void Spark() {
		sparks.Play();
	}
}
