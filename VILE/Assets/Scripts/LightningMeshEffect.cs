using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningMeshEffect : MonoBehaviour {

	private MeshRenderer[] renderers;
	private Light lightningLight;
	int rand;

	// Use this for initialization
	void Start () {
		renderers = GetComponentsInChildren<MeshRenderer>();
		foreach(MeshRenderer r in renderers) {
			r.enabled = false;
		}
		lightningLight = transform.parent.GetComponentInChildren<Light>();
	}
	
	// Update is called once per frame
	void Update () {
		renderers[rand].enabled = false;
		rand = Mathf.FloorToInt(Random.Range(0, renderers.Length));
		renderers[rand].enabled = true;
		transform.Rotate(Vector3.forward, (float) rand / renderers.Length * 360);
		if(rand < renderers.Length / 4) lightningLight.enabled = true;
		else lightningLight.enabled = false;
	}

	public void Deactivate() {
		lightningLight.enabled = false;
		gameObject.SetActive(false);
	}
}
