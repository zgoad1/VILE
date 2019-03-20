using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footprint : PooledObject {

	public Transform foot;
	public Vector3 offset;
	public float duration = 2f;
	private Vector3 iScale;
	private Vector3 newScale = Vector3.one;

	private Material material {
		get {
			return GetComponent<MeshRenderer>().material;
		}
	}
	private Color newColor = Color.white;
	private Color newEmission = Color.white;



	private void Awake() {
		foot = Helper.RecursiveFind(GameController.player.transform, "Toes_" + name.Substring(10, 1));
		iScale = transform.localScale;
	}

	public override void Restart() {
		base.Restart();
		transform.forward = -foot.transform.right;
		transform.position = foot.position + offset;
		newColor = Color.white;
		transform.localScale = iScale;
		StopCoroutine("FadeOut");
		StartCoroutine("FadeOut");
	}

	private IEnumerator FadeOut() {
		float frame = 1f / 60f;
		for(float i = 0; i < duration; i += frame) {
			float func = 1f / (10f * (i + 0.01f)) + (i / 1) * 5;
			newScale.x = iScale.x * func;
			newScale.z = iScale.z * func;
			transform.localScale = newScale;
			newColor.a = newEmission.r = newEmission.g = newEmission.b = 1 - i / duration;
			material.color = newColor;
			material.SetColor("_EmissionColor", newEmission * 3);
			yield return new WaitForSeconds(frame);
		}
		gameObject.SetActive(false);
	}
}

