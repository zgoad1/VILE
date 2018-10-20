using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeWhenClose : MonoBehaviour {

	[HideInInspector] public bool visible = true;
	public Renderer r;

	private void Reset() {
		Renderer[] rs = GetComponentsInChildren<Renderer>();
		foreach(Renderer rend in rs) {
			if(rend.gameObject.GetComponent<FadeWhenClose>() == null) rend.gameObject.AddComponent<FadeWhenClose>();
		}
		if(r == null) r = GetComponentInChildren<Renderer>();
	}

	void Start () {
		Reset();
		if(r != null) {
			foreach(Material m in r.materials) {
				m.SetInt("_ZWrite", 1);
			}
		}
		if(GetComponent<Rigidbody>() == null) {
			Rigidbody rb = gameObject.AddComponent<Rigidbody>();
			rb.useGravity = false;
			rb.isKinematic = true;
		}
	}

	public IEnumerator FadeOut(int frames) {
		StopCoroutine("FadeIn");
		//Debug.Log("Renderer: " + r);
		if(r != null) {
			// initial i corresponds to current alpha
			for(float i = (1 - r.material.color.a) * frames; i < frames; i++) {
				yield return null;
				foreach(Material m in r.materials) {
					m.color = new Color(m.color.r, m.color.g, m.color.b, 1 - i / frames);
				}
			}
			foreach(Material m in r.materials) {
				m.color = new Color(m.color.r, m.color.g, m.color.b, 0);
				//Debug.Log("Fading material: " + m);
			}
			visible = false;
		}
	}

	public IEnumerator FadeIn(int frames) {
		StopCoroutine("FadeOut");
		if(r != null) {
			for(float i = r.material.color.a * frames; i < frames; i++) {
				yield return null;
				foreach(Material m in r.materials) {
					m.color = new Color(m.color.r, m.color.g, m.color.b, i / frames);
				}
			}
			foreach(Material m in r.materials) {
				m.color = new Color(m.color.r, m.color.g, m.color.b, 1);
			}
			visible = true;
		}
	}
}
