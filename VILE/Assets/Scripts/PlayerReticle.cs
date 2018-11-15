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
	[SerializeField] private Sprite defaultSprite;
	[SerializeField] private Sprite targetSprite;
	private Quaternion defaultRotation = Quaternion.identity;
	private Quaternion possessableRotation = Quaternion.Euler(0, 0, 45);
	private Quaternion newRotation = Quaternion.identity;
	private Image image;
	private Color newColor = Color.white;

	private void Reset() {
		player = FindObjectOfType<Player>();
		transform = GetComponent<RectTransform>();
		canvas = FindObjectOfType<Canvas>().GetComponent<RectTransform>();
		image = GetComponent<Image>();
	}

	// Use this for initialization
	void Start () {
		Reset();
	}
	
	// Update is called once per frame
	void LateUpdate () {
		// set position based on target
		if(player.target != null) {
			newPos = ((Enemy)player.target).screenCoords + originOffset;
			newPos.x *= Screen.width * (canvas.rect.width / Screen.width);
			newPos.y *= Screen.height * (canvas.rect.height / Screen.height);
		} else {
			newPos = Vector3.zero;
		}

		// set color and rotation based on target
		image.enabled = true;
		if(player.target != null) {
			image.sprite = targetSprite;
			if(player.target.control == Controllable.state.STUNNED) {
				if(GameController.frames % 2 == 0) {
					image.enabled = false;
					newRotation = possessableRotation;
					// rotate periodically to be more attention-grabbing
					if(GameController.frames % 16 == 0) {
						possessableRotation = Quaternion.Euler(possessableRotation.eulerAngles + new Vector3(0, 0, -90));
					}
				}
			} else {
				newRotation = defaultRotation;
			}
		} else {
			image.sprite = defaultSprite;
			newRotation = defaultRotation;
		}

		// disappear when we sprint
		if(player.isLightning) {
			newColor.a = 0;
		} else {
			newColor.a = Mathf.Min(1, newColor.a + 0.05f);
		}

		image.color = newColor;
		transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, 0.3f);
		transform.anchoredPosition = Vector2.Lerp(transform.anchoredPosition3D, newPos, 0.5f * 60 * Time.deltaTime);
	}
}
