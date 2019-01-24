using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBar : MonoBehaviour {

	public Controllable character;
	public float value = 100;
	[HideInInspector] public float maxValue = 100;

	private Vector3 newScale = Vector3.one;

	private void Start() {
		transform.localScale = newScale;
	}

	private void LateUpdate() {
		newScale.x = Mathf.Lerp(newScale.x, value / maxValue, 0.4f * 60 * Time.deltaTime);
		transform.localScale = newScale;
	}
}
