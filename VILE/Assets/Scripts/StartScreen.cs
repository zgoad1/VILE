using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**This is the scene that makes the smooth transition past the Unity logo.
 * Camera clears to logo background color and we immediately fade out to the title screen.
 */

public class StartScreen : MonoBehaviour {
	// Use this for initialization
	void Start () {
		GameController.SceneChange("TitleScreen");
	}
}
