using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleController : MonoBehaviour {
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Space)) {
			// TODO: Have some way to disable the intro
			// One easy way would be to only let it activate once per session
			GameController.SceneChange("Intro");
		}
	}
}
