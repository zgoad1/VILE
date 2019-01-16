using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

	public static GameController instance;
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
	// The exact point on the player at which enemies should attack. Needed for precise raycasting.
	public static Transform playerTarget;
	public static int defaultLayerMask;

	// laser barriers
	[SerializeField] private Material laserBarrier;
	private Color newColorLB;
	private float initialAlphaLB;

	// wall glow animation
	[SerializeField] private Material wallGlow;
	private Color newColorWG = Color.black;

	[SerializeField] private int numPremadeObjects = 10;    // how many of each object to pool
	[SerializeField] private List<GameObject> objectList;   // said objects
	private static List<GameObject> objectPool = new List<GameObject>();

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
		playerTarget = FindObjectOfType<Player>().transform.Find("Target Transform");
		defaultLayerMask = 1 << LayerMask.NameToLayer("Solid") | 1 << LayerMask.NameToLayer("Characters") | 1 << LayerMask.NameToLayer("Default");
	}

	// Use this for initialization
	void Awake () {
		Reset();
		if(number == 0) {
			instance = this;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}
		number++;

		if(SceneManager.GetActiveScene().name == "Level") {
			CreateObjectPool();
		}
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
		if(nextScene.ToString() == "Level") {
			instance.CreateObjectPool();
		}
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

	// Create a bunch of objects that will need to be instantiated over and over
	// so we can pretend to do that while saving resources
	private void CreateObjectPool() {
		for(int i = 0; i < numPremadeObjects; i++) {
			foreach(GameObject ob in objectList) {
				GameObject newOb = Instantiate(ob);
				objectPool.Add(newOb);
				newOb.SetActive(false);
			}
		}
	}

	public static void InstantiateFromPool(GameObject ob, Transform transform) {
		if(instance.objectList.Contains(ob)) {
			foreach(GameObject o in objectPool) {
				// Pretend to instantiate the first matching object in the pool
				if(o.name.Remove(o.name.Length - 7) == ob.name) {   // remove "(Clone)" and compare names
					o.GetComponent<PooledObject>().Restart();

					// copy transform of original object
					o.transform.SetParent(transform.parent);
					o.transform.localPosition = transform.localPosition;
					o.transform.localRotation = transform.localRotation;
					o.transform.localScale = transform.localScale;

					// move to back of list
					objectPool.Remove(o);
					objectPool.Add(o);

					return;
				}
			}
		} else {
			Debug.LogError("Tried to instantiate pooled object that isn't pooled");
		}
	}
}
