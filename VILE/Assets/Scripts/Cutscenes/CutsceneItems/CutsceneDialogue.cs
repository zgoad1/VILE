using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CutsceneDialogue : EventItem {

	protected DialogueBox dbox;
	[SerializeField] protected DialogueArray[] dialogue;
	[SerializeField] protected bool useCutsceneDbox = true;	// Cutscenes by default use the cutscene dbox.

	protected override void Reset() {
		dbox = FindObjectOfType<CutsceneDbox>();
	}
	
	protected override void Start () {
		Reset();
		dbox.ShowDialogue(dialogue[0].items);
		Destroy(gameObject);
	}
}
