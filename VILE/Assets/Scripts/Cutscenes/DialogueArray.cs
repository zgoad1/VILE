using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueArray {
	public FaceText[] items = new FaceText[1];
	public GameObject _event;
	public bool turnBody = false;
	public bool turnHead = true;

	public DialogueArray() {
		items = new FaceText[1];
	}

	public DialogueArray(FaceText[] i) {
		items = i;
	}
}
