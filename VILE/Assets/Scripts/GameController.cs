using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

	public static Player player;
	public static MainCamera mainCam;
	public static Camera mainCamCam;
	public static CameraControl camControl;
	public static GameObject enemyHpBarObject;
	public static RectTransform UICanvas;
	private static Animator blackfade;
	public static int frames = 0;
	private static int number = 0;
	private static string nextScene;
	public static bool paused = false;

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
		if(mainCam != null) mainCamCam = mainCam.GetComponent<Camera>();
		camControl = FindObjectOfType<CameraControl>();
	}

	private void Reset() {
		FindPlayer();
		frames = 0;
		newColorLB = laserBarrier.color;
		initialAlphaLB = newColorLB.a;
		blackfade = GameObject.Find("Blackfade").GetComponent<Animator>();
		enemyHpBarObject = Resources.Load<GameObject>("Enemy HP");
		UICanvas = FindObjectOfType<Canvas>().GetComponent<RectTransform>();
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

		#region Pause

		if(Input.GetButtonDown("Pause")) {
			if(!paused) Pause();
			else Unpause();
		}
		#endregion

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

	public static void Pause() {
		paused = true;
		Controllable[] characters = FindObjectsOfType<Controllable>();
		foreach(Controllable c in characters) {
			c.enabled = false;
			c.anim.speed = 0;
		}
		player.readInput = false;
		player.velocity = Vector3.zero;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		Time.timeScale = 0;
	}

	public static void Unpause() {
		paused = false;
		Controllable[] characters = FindObjectsOfType<Controllable>();
		foreach(Controllable c in characters) {
			c.enabled = true;
			c.anim.speed = 1;
		}
		player.readInput = true;
		camControl.readInput = true;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		Time.timeScale = 1;
	}
}
