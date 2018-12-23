using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopMusic : EventItem {

	[SerializeField] private string music;
	[SerializeField] private int fadeTime = 180;

	// Use this for initialization
	protected override void Start () {
		base.Start();
		FindObjectOfType<AudioManager>().FadeOut(music, fadeTime);
		eq.Dequeue();
		Destroy(gameObject);
	}
}
