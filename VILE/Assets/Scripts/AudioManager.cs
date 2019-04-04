using UnityEngine.Audio;
using System;
using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public static AudioManager instance;

    [SerializeField] private Sound[] sounds;

	// Use this for initialization
	void Awake () {
		
        if(instance == null) {
            instance = this;
            Debug.Log("Creating audio manager");

			DontDestroyOnLoad(gameObject);

			foreach(Sound s in sounds) {
				s.source = gameObject.AddComponent<AudioSource>();
				s.source.clip = s.clip;
				s.source.volume = s.volume;
				s.source.pitch = s.pitch;
				s.source.loop = s.loop;
				Debug.Log("Adding audio source: " + s.source.clip.name);
			}
		} else {
            Destroy(gameObject);
            Debug.Log("Destroying extra audio manager");
            return;
		}
	}

	public void Play(string name) {
		Sound sound = Array.Find(sounds, s => s.name == name);
		if(sound != null) {
			Debug.Log("Playing sound: " + sound.name + "\nSource: " + sound.source);
			sound.source.Play();
			AudioListener.volume = 100;
			Debug.Log("Volume: " + AudioListener.volume);
		} else {
			Debug.LogError("AUDIO: could not find sound: " + name);
		}
	}

    public void Stop(string name) {
		Sound sound = Array.Find(sounds, s => s.name == name);
		if(sound != null) {
			sound.source.Stop();
		} else {
			Debug.LogError("AUDIO: could not find sound: " + name);
		}
	}

	IEnumerator FadeInTrack(string name, int fadeFrames) {
		Sound sound = Array.Find(sounds, s => s.name == name);
		for(float i = 0; i < fadeFrames; i++) {
			sound.source.volume = (i + 1) / fadeFrames * sound.volume;
			yield return null;
		}
	}

	IEnumerator FadeOutTrack(string name, int fadeFrames) {
		Sound sound = Array.Find(sounds, s => s.name == name);
		for(float i = 0; i < fadeFrames; i++) {
			sound.source.volume = (fadeFrames - i) / fadeFrames * sound.volume;
			yield return null;
		}
		Stop(name);
		//Debug.Log("Faded out " + name);
	}

	public void FadeIn(string name, int fadeFrames) {
		IEnumerator cr = FadeInTrack(name, fadeFrames);
		StartCoroutine(cr);
	}

	public void FadeOut(string name, int fadeFrames) {
		IEnumerator cr = FadeOutTrack(name, fadeFrames);
		StartCoroutine(cr);
	}

	private void PrintSources(string str) {
		Debug.Log(str);
		foreach(Sound s in sounds) {
			Debug.Log("SOUND: " + s.name + "\nSOURCE: " + s.source);
		}
	}
	private void PrintSources() {
		PrintSources("");
	}
}
