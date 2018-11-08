using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerReticle : MonoBehaviour {

	private Vector3 newPos = Vector3.zero;
	private Player player;
	private new RectTransform transform;
	private Vector3 originOffset = new Vector3(-0.5f, -0.5f, 0);
	private RectTransform canvas;

	private void Reset() {
		player = FindObjectOfType<Player>();
		transform = GetComponent<RectTransform>();
		canvas = FindObjectOfType<Canvas>().GetComponent<RectTransform>(); ;
	}

	// Use this for initialization
	void Start () {
		Reset();
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if(player.target != null) {
			newPos = ((Enemy)player.target).screenCoords + originOffset;
			newPos.x *= Screen.width * (canvas.rect.width / Screen.width);
			newPos.y *= Screen.height * (canvas.rect.height / Screen.height);
			Debug.Log("Moving reticle");
		} else {
			newPos = Vector3.zero;
			Debug.Log("Null target");
		}
		transform.anchoredPosition = Vector2.Lerp(transform.anchoredPosition3D, newPos, 0.5f);
	}
}
