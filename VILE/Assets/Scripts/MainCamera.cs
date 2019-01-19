using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour {

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
}
