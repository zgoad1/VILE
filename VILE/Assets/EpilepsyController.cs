using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EpilepsyController : MonoBehaviour {

	private static Camera mainCam;
	//private static Camera charCam;
	private static Color c1;
	private static Color c2;
	private bool toStop;

	private void Reset() {
		mainCam = FindObjectOfType<MainCamera>().GetComponent<Camera>();
		//charCam = GameObject.Find("Character Camera").GetComponent<Camera>();
	}

	// Use this for initialization
	void Start () {
		Reset();
	}

	public void SetBackground(Color c) {
		RenderSettings.fogColor = c;
		mainCam.backgroundColor = c;
	}

	/**Start a screen flash for a specified amount of frames,
	 * flashing between black and the two colors.
	 * Flashes indefinitely if duration is negative.
	 */
	public void FlashStart(Color c1, Color c2, int duration) {
		//FlashStop();
		EpilepsyController.c1 = c1;
		EpilepsyController.c2 = c2;
		IEnumerator cr = StartFlashing(duration);
		StartCoroutine(cr);
	}

	public void FlashStop() {
		toStop = true;
	}

	private IEnumerator StartFlashing(int frames) {
		SetBackground(c1);
		yield return null;
		yield return null;
		SetBackground(Color.black);
		yield return null;
		yield return null;
		yield return null;
		SetBackground(c2);
		yield return null;
		yield return null;
		SetBackground(Color.black);
		yield return null;
		yield return null;
		/*
		if(frames % 10 == 0) {
			// Second color
			SetBackground(c2);
			Debug.Log("c2");
		} else if(frames % 8 == 0) {
			// black
			SetBackground(Color.black);
			Debug.Log("Black");
		} else if(frames % 5 == 0) {
			// first color
			SetBackground(c1);
			Debug.Log("c1");
		} else if(frames % 3 == 0) {
			// black
			SetBackground(Color.black);
			Debug.Log("Black");
		}
		*/
		/*
		if(frames % 4 == 0) SetBackground(c1);
		else if(frames % 3 == 0) SetBackground(Color.black);
		else if(frames % 2 == 0) SetBackground(c2);
		else SetBackground(Color.black);
		*/
		yield return null;
		frames--;

		if(frames == -1 || toStop) {
			SetBackground(Color.black);
			toStop = false;
		} else {
			IEnumerator cr = StartFlashing(frames);
			StartCoroutine(cr);
		}
	}
}
