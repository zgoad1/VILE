using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventQueue : MonoBehaviour {

	private Queue<EventItem> q = new Queue<EventItem>();
	[SerializeField] private EventItem[] queue;

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(gameObject);
		foreach(EventItem i in queue) {
			q.Enqueue(i);
		}
		Dequeue();
	}

	public void WaitFrames(int frames) {
		IEnumerator cr = WaitFramesCR(frames);
		StartCoroutine(cr);
	}

	public void Dequeue() {
		if(q.Count != 0) {
			Debug.Log("DQing");
			EventItem ob = q.Dequeue();
			if(ob != null) {
				Instantiate(ob);
			} else {
				StartCoroutine("Wait");
			}
		} else {
			Destroy(gameObject);
			Debug.Log("Ending cutscene");
		}
	}

	private IEnumerator Wait() {
		yield return new WaitForSeconds(1f);
		Dequeue();
	}

	private IEnumerator WaitFramesCR(int frames) {
		for(int i = 0; i < frames; i++) yield return null;
		Dequeue();
	}
}
