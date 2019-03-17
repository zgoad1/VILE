using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

	#region Static
	// Singleton objects
	public static GameController instance;
	public static Player player;
	public static MainCamera mainCam;
	public static Camera mainCamCam;
	public static CameraControl camControl;

	// Global GameObjects
	public static GameObject enemyHpBarPrefab {
		get {
			if(instance == null) {
				instance = GameObject.Find("Game Controller").GetComponent<GameController>();
			}
			return instance._enemyHpBarPrefab;
		}
	}
	public static GameObject conductorPrefab {
		get {
			if(instance == null) {
				instance = GameObject.Find("Game Controller").GetComponent<GameController>();
			}
			return instance._conductorPrefab;
		}
	}

	// Folders
	public static Transform enemyParent;
	public static Transform objectPoolParent;
	public static Transform clawRendererParent;

	// Misc.
	public static RectTransform UICanvas;
	private static Animator blackfade;
	public static int frames = 0;
	private static int number = 0;
	private static string nextScene;
	public static bool paused = false;
	// The exact point on the player at which enemies should attack. Needed for precise raycasting.
	public static Transform playerTarget;
	public static int defaultLayerMask;
	public static int solidLayer;
	public static int enemyLayer;
	#endregion

	#region Materials
	[Header("Materials")]
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

	// tunnel lights
	[SerializeField] private Material tunnelGlow;
	private Vector2 tunnelGlowOffset = Vector2.zero;

	// any animated material that just scrolls vertically
	[SerializeField] private ScrollingMaterial[] scrolling;
	#endregion

	#region Singleton objects
	[Header("Singleton objects")]
	public GameObject _enemyHpBarPrefab;
	public GameObject _conductorPrefab;
	#endregion
	
	[Header("Object pool")]
	[SerializeField] private int numPremadeObjects = 10;    // how many of each object to pool
	[SerializeField] private List<GameObject> objectList;   // said objects
	private static List<GameObject> objectPool = new List<GameObject>();

	private static float ifov;
	private static bool isLevel = false;    // whether we're currently in the playable level
	[HideInInspector] public static GameObject stunSparksPrefab;



	public void FindPlayer() {
		player = FindObjectOfType<Player>();
		if(player != null) {
			playerTarget = player.transform.Find("Target Transform");
		}
		mainCam = FindObjectOfType<MainCamera>();
		if(mainCam != null) {
			mainCamCam = mainCam.GetComponent<Camera>();
			ifov = mainCamCam.fieldOfView;
		}
		camControl = FindObjectOfType<CameraControl>();
	}

	private void Reset() {
		FindPlayer();
		frames = 0;
		newColorLB = laserBarrier.color;
		initialAlphaLB = newColorLB.a;
		blackfade = GameObject.Find("Blackfade").GetComponent<Animator>();
		UICanvas = FindObjectOfType<Canvas>().GetComponent<RectTransform>();
		defaultLayerMask = 1 << LayerMask.NameToLayer("Solid") | 1 << LayerMask.NameToLayer("Characters") | 1 << LayerMask.NameToLayer("Default");
		stunSparksPrefab = objectList.Find(g => g.name == "StunSparks");
		enemyLayer = LayerMask.NameToLayer("Enemies");
		solidLayer = LayerMask.NameToLayer("Solid");
		enemyParent = GameObject.Find("Enemies").transform;
		objectPoolParent = GameObject.Find("Object Pool").transform;
		clawRendererParent = GameObject.Find("Claw Renderers").transform;
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

		Time.timeScale = 1; // Suddenly became necessary on 1-27-19. All other references to Time.timeScale were checked; none happen.
							// Found the problem: the game had been stopped when the timescale was at 0. It does not get reset on exiting
							// play mode. (Unity bug)
		OnLevelWasLoaded(-1);
	}

	// Instantly change scene to nextScene
	public static void LoadNextScene() {
		SceneManager.LoadScene(nextScene);
	}

	private void OnLevelWasLoaded(int level) {
		FindPlayer();
		if(SceneManager.GetActiveScene().name == "Level") {
			instance.CreateObjectPool();
			isLevel = true;
		} else {
			isLevel = false;
		}
	}

	// Update is called once per frame
	void Update () {
		if(isLevel) {
			#region Material animations

			// wall glow
			newColorWG.r = 0.5f * Mathf.Sin(2 * Mathf.PI / 4 * Time.time) + 0.5f;
			wallGlow.SetColor("_EmissionColor", newColorWG);

			// tunnel wall lights
			tunnelGlowOffset.y = 1 * Mathf.Sin(2 * Mathf.PI / 6.5f * Time.time);
			tunnelGlow.mainTextureOffset = tunnelGlowOffset;

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
			foreach(ScrollingMaterial m in scrolling) {
				m.material.mainTextureOffset += m.scrollSpeed * 60 * Time.deltaTime;
			}
			//conductorMaterial.mainTextureOffset += verticalScrollSpeed;

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

	public static void Pause() {
		paused = true;
		Controllable[] characters = FindObjectsOfType<Controllable>();
		foreach(Controllable c in characters) {
			c.enabled = false;
			if(c.anim != null) c.anim.speed = 0;
		}
		player.readInput = false;
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
				newOb.transform.SetParent(objectPoolParent);
				newOb.SetActive(false);
			}
		}
	}

	public static GameObject InstantiateFromPool(GameObject ob, Transform transform) {
		if(instance.objectList.Contains(ob)) {
			foreach(GameObject o in objectPool) {
				// Pretend to instantiate the first matching object in the pool
				if(o.name.Remove(o.name.Length - 7) == ob.name) {   // remove "(Clone)" and compare names
					o.GetComponent<PooledObject>().Restart();

					// copy transform of original object
					o.transform.position = transform.position;
					o.transform.rotation = transform.rotation;
					o.transform.localScale = transform.localScale;

					// move to back of list
					objectPool.Remove(o);
					objectPool.Add(o);

					return o;
				}
			}
			Debug.LogError("You somehow just tried to instantiate an object that both is and is not pooled.");
			return null;
		} else {
			Debug.LogError("Tried to InstantiateFromPool object that isn't pooled");
			return null;
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
