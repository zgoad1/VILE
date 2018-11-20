using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blackfade : MonoBehaviour {
	public void OnAnimationFinish() {
		GameController.LoadNextScene();
	}
}
