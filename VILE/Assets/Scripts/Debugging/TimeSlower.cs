using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeSlower : MonoBehaviour {

	public float timeScale = 0.2f;
	public bool applyEveryFrame = false;

	// Start is called before the first frame update
	void Start() {
		Time.timeScale = timeScale;
	}

	private void Update() {
		if(applyEveryFrame) {
			Time.timeScale = timeScale;
		}
	}
}
