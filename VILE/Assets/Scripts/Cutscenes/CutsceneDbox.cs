using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneDbox : DialogueBox {

	private EventQueue eq;
	private Vector3 posNormal = Vector3.zero;
	private Vector3 posChar = new Vector3(0, -232, 0);

	protected override void Awake() {
		base.Awake();
		eq = FindObjectOfType<EventQueue>();
	}

	protected override void Finish() {
		base.Finish();
		StartCoroutine("Dq");
	}

	private IEnumerator Dq() {
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		// Dequeue next event
		if(eq != null) eq.Dequeue();
	}

	public override void ShowDialogue(FaceText[] items) {
		base.ShowDialogue(items);
		if(items[0].face != null) {
			// If we start with a face, move the dbox down (assume characters are talking and we need to see the middle of the screen).
			transform.localPosition = posChar;
		} else {
			transform.localPosition = posNormal;
		}
	}
}
