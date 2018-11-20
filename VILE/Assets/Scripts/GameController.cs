using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

	public static Player player;
	public static MainCamera mainCam;
	public static CameraControl camControl;
	private static Animator blackfade;
	public static int frames = 0;
	private static int number = 0;
	private static string nextScene;

	// laser barriers
	[SerializeField] private Material laserBarrier;
	private Color newColorLB;
	private float initialAlphaLB;

	// wall glow animation
	[SerializeField] private Material wallGlow;
	private Color newColorWG = Color.black;

	public void FindPlayer() {
		player = FindObjectOfType<Player>();
		mainCam = FindObjectOfType<MainCamera>();
		camControl = FindObjectOfType<CameraControl>();
	}

	private void Reset() {
		FindPlayer();
		frames = 0;
		newColorLB = laserBarrier.color;
		initialAlphaLB = newColorLB.a;
		blackfade = GameObject.Find("Blackfade").GetComponent<Animator>();
	}

	// Use this for initialization
	void Awake () {
		Reset();
		if(number == 0) {
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}
		number++;
	}
	
	// Update is called once per frame
	void Update () {
		newColorWG.r = 0.5f * Mathf.Sin(2 * Mathf.PI / 4 * Time.time) + 0.5f;
		wallGlow.SetColor("_EmissionColor", newColorWG);

		newColorLB.a = 0.1f * Mathf.Sin(2 * Mathf.PI / 0.12f * Time.time) + .9f;
		laserBarrier.color = newColorLB;

		frames++;
	}

	private void OnDestroy() {
		newColorWG.r = 0;
		wallGlow.SetColor("_EmissionColor", newColorWG);

		newColorLB.a = initialAlphaLB;
		laserBarrier.color = newColorLB;
	}

	// Fade screen out, which will automatically call LoadNextScene
	public static void SceneChange(string newScene) {
		nextScene = newScene;
		blackfade.SetTrigger("fadeOut");
	}

	// Instantly change scene to nextScene
	public static void LoadNextScene() {
		SceneManager.LoadScene(nextScene);
	}
}
