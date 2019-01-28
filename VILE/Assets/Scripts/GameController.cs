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

	// arc stone animation
	[SerializeField] private Material arcStone;
	private Vector2 arcStoneOffset = Vector2.zero;
	private Color newColorAS = Color.gray;

	// conductor animation
	[SerializeField] private Material conductor;
	private Vector2 conductorScrollSpeed = new Vector2(0, 0.02f);

	[SerializeField] private int numPremadeObjects = 10;    // how many of each object to pool
	[SerializeField] private List<GameObject> objectList;   // said objects
	private static List<GameObject> objectPool = new List<GameObject>();

	private static float ifov;

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
		ifov = mainCamCam.fieldOfView;
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

		Time.timeScale = 1;	// Suddenly became necessary on 1-27-19. All other references to Time.timeScale were checked; none happen.
	}
	
	// Update is called once per frame
	void Update () {
		#region Material animations
		// wall glow
		newColorWG.r = 0.5f * Mathf.Sin(2 * Mathf.PI / 4 * Time.time) + 0.5f;
		wallGlow.SetColor("_EmissionColor", newColorWG);

		// laser barrier
		newColorLB.a = 0.1f * Mathf.Sin(2 * Mathf.PI / 0.12f * Time.time) + .9f;
		laserBarrier.color = newColorLB;

		// arc stones
		arcStoneOffset.y = 0.12f * Mathf.Sin(2 * Mathf.PI / 8 * Time.time);
		arcStone.mainTextureOffset = arcStoneOffset;
		float newColorASColor = 1.5f + 0.5f * Mathf.Sin(2 * Mathf.PI / 5 * Time.time);
		newColorAS.r = newColorAS.g = newColorAS.b = newColorASColor;
		arcStone.SetColor("_EmissionColor", newColorAS);

		// conductor
		conductor.mainTextureOffset += conductorScrollSpeed;
		#endregion

		#region Pause
		if(Input.GetButtonDown("Pause")) {
			if(!paused) Pause();
			else Unpause();
		}
		#endregion

		mainCamCam.fieldOfView = Mathf.Lerp(mainCamCam.fieldOfView, ifov, 0.2f * 60 * Time.deltaTime);

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
			if(c.anim != null) c.anim.speed = 0;
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
			if(c.anim != null) c.anim.speed = 1;
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

	public static void HitStop(float intensity) {
		//Debug.Log("Hit stop intensity: " + intensity);
		IEnumerator cr = instance.HitStopCR(intensity);
		instance.StartCoroutine(cr);
		camControl.ScreenShake(intensity * 2);
		CamZoom(18 * intensity + 2);
		//camControl.Zoom(intensity * 500);
	}

	private IEnumerator HitStopCR(float intensity) {
		Time.timeScale = 0.1f;
		yield return new WaitForSecondsRealtime(intensity);
		if(!paused) Time.timeScale = 1;
	}

	public static void CamZoom(float intensity) {
		mainCamCam.fieldOfView = ifov - intensity;
	}
}
