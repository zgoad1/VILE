using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningMeshEffect : MonoBehaviour {

	private Player player;
	private MeshRenderer[] renderers;
	private Light lightningLight;
	private int rand;
	private Vector3 scale;
	private float length = 32;
	int rand;

	// Use this for initialization
	void Start () {
		renderers = GetComponentsInChildren<MeshRenderer>();
		foreach(MeshRenderer r in renderers) {
			r.enabled = false;
		}
		lightningLight = transform.parent.GetComponentInChildren<Light>();
		player = FindObjectOfType<Player>();
		scale = transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
		if(player.target != null) {
			renderers[rand].enabled = false;
			rand = Mathf.FloorToInt(Random.Range(0, renderers.Length));
			renderers[rand].enabled = true;
			transform.Rotate(Vector3.forward, (float) rand / renderers.Length * 360);
			if(rand < renderers.Length / 4) lightningLight.enabled = true;
			else lightningLight.enabled = false;
			transform.LookAt(player.target.transform);
			scale.z = Vector3.Distance(player.transform.position, player.target.transform.position) / length;
			transform.localScale = scale;
		} else {
			Deactivate();
		}
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
