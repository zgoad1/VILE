using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneSound : EventItem {
	
	private AudioManager am;
	[SerializeField] private string sound;
	[SerializeField] private bool fadeIn = false;
	[SerializeField] private int fadeFrames = 60;

	// Use this for initialization
	protected override void Start () {
		base.Start();
		am = FindObjectOfType<AudioManager>();
		am.Play(sound);
		if(fadeIn) am.FadeIn(sound, fadeFrames);
		eq.Dequeue();
		Destroy(gameObject);
	}
}
