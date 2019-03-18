using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour {

	private Animator anim;
	private Transform iParent;

	private void Start() {
		anim = GetComponent<Animator>();
		iParent = transform.parent;
	}

	private void OnTriggerEnter(Collider collision) {
		//Debug.Log("Camera hit a " + collision.gameObject);
		FadeWhenClose fader = collision.gameObject.GetComponent<FadeWhenClose>();
		if(fader != null) {
			fader.StartCoroutine("FadeOut", 8);
		}
	}

	private void OnTriggerExit(Collider collision) {
		FadeWhenClose fader = collision.gameObject.GetComponent<FadeWhenClose>();
		if(fader != null) {
			fader.StartCoroutine("FadeIn", 8);
		}
	}

	public void ScreenShake(float intensity) {
		GameController.camControl.ScreenShake(intensity);
	}

	// Disable the animator and preserve our transform
	private void AnimFunc_OnConductorInFinish() {
		Vector3 newPos = transform.position;
		Quaternion newRot = transform.rotation;
		anim.enabled = false;
		transform.position = newPos;
		transform.rotation = newRot;
		GameController.camControl.ShowArrows();
		//ResetParent();
	}

	private void AnimFunc_OnConductorOutFinish() {
		GameController.camControl.FinishConducting();
		anim.enabled = false;
		//ResetParent();
	}

	public void ResetParent() {
		transform.SetParent(iParent);
	}
}
