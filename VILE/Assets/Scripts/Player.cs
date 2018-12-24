using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Controllable {
	private ParticleSystem lightning;
	private ParticleSystem burst;
	private ParticleSystem head;
	private TrailRenderer[] handTrails;
	[HideInInspector] public bool isLightning = false;
	private Transform sprintCam;
	private EpilepsyController flasher;
	private static Vector2 screenCenter = new Vector2(0.5f, 0.5f);
	private bool possessing = false;
	private Enemy possessed = null;
	//private LightningMeshEffect a2fx;
	private ParticleSystem a2fx;
	private bool canSprint = true;
	private LayerMask rayMask;
	private LayerMask solidLayer;
	private AttackHitbox ahbL, ahbR;
	private int comboNumber = 0;
	public UIBar stBar;
	private float rechargeFactor = 0.05f;

	// temp
	[HideInInspector] public Vector3 iPos = Vector3.zero;

	public static List<Targetable> targets = new List<Targetable>();

	protected override void Reset() {
		base.Reset();
		lightning = GetComponentsInChildren<ParticleSystem>()[2];
		burst = GetComponentsInChildren<ParticleSystem>()[3];
		head = GetComponentsInChildren<ParticleSystem>()[4];
		sprintCam = GameObject.Find("SprintCam").transform;
		flasher = FindObjectOfType<EpilepsyController>();
		a2fx = GetComponentsInChildren<ParticleSystem>()[5];
		solidLayer = LayerMask.NameToLayer("Solid");
		rayMask = 1 << LayerMask.NameToLayer("Solid");
		handTrails = GetComponentsInChildren<TrailRenderer>();
		//ahbL = GetComponentsInChildren<AttackHitbox>()[0];
		//ahbR = GetComponentsInChildren<AttackHitbox>()[1];
		hpBar = GameObject.Find("Tess HP").GetComponent<UIBar>();//FindObjectOfType<UIBar>();
		hpBar.character = this;
		hpBar.maxValue = maxHP;
		stBar = GameObject.Find("Tess St").GetComponent<UIBar>();
		stBar.character = this;
		stBar.maxValue = 100;
	}

	protected override void Start() {
		base.Start();
		
		prevPosition = transform.position;
		camTransform.position = transform.position;

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		atk2Cost = 25;

		SetPlayer();
	}

	protected override void Update() {
		if(transform.position.y < -1) transform.position = iPos;
		if(!possessing) base.Update();
		else {
			transform.position = possessed.transform.position;
			transform.rotation = mainCam.transform.rotation;

			SetTarget();

			if(Input.GetButtonDown("Run")) {
				Unpossess(true);
			}
		}
	}

	protected override void PlayerUpdate() {

		base.PlayerUpdate();
		
		SetMotion();
		// Set the target dynamically if we're not attacking.
		// If we're attacking, only set the target when we try to move forward.
		if(!attacking) {
			SetHandTrails(false);
			SetTarget();
		} else {
			if(target != null) a2fx.transform.LookAt(target.camLook);
			else a2fx.transform.forward = camTransform.forward;
			if(fwdKey > 0) {
				Targetable prevTarget = target;
				SetTarget();
				if(target == null) target = prevTarget;
			}
		}

		if(stamina <= 20 - rechargeFactor) stamina += rechargeFactor;

		// home in on an enemy to possess it
		if(sprinting && CanPossessTarget()) {
			control = state.AI;
		}
	}

	// use this for homing in on enemies
	protected override void AIUpdate() {
		if(Input.GetButtonUp("Run")) {
			// switch state
			control = state.PLAYER;
		}
		cc.Move((target.transform.position - transform.position).normalized * (runSpeed / 2f) * 60 * Time.smoothDeltaTime);
	}

	protected override void OnControllerColliderHit(ControllerColliderHit hit) {
		base.OnControllerColliderHit(hit);  // sets onGround
		if(hit.gameObject.GetComponent<Enemy>() == target && control == state.AI) {
			Debug.Log("Possessing " + target);
			Possess((Enemy)target);
		} /*else if(!onGround && hit.gameObject.layer == solidLayer && velocity.y < -0.1f) {
			cam.ScreenShake(Mathf.Abs(velocity.y));
		}*/	// decided I like how she feels lighter when she lands
		// make a "checkpoint" to keep from falling off map
		if(hit.gameObject.layer == LayerMask.NameToLayer("Solid") && Mathf.Floor(Time.time) % 2 == 0) iPos = transform.position;
	}

	/**Set the target to the closest enemy in the targets array
	 */
	protected override void SetTarget() {
		if(target is Enemy) ((Enemy)target).hpBar.gameObject.SetActive(false);
		// add any onscreen enemies that are close enough to the center of the screen
		// to the targets array (and remove those who aren't)
		foreach(Targetable t in onScreen) {
			bool inRange = IsInRange(t);
			// don't consider the player (if possessing an enemy)
			// don't consider things behind the player when the camera is behind the player
			if(t != possessed /*&& (t.IsInFrontOf(this) || !t.IsInFrontOf(this) && Helper.IsInFrontOf(GameController.mainCam.transform, t.transform))*/) {
				if(!t.isTarget && inRange) {
					t.isTarget = true;
					targets.Add(t);
				} else if(t.isTarget && !inRange) {
					t.isTarget = false;
					targets.Remove(t);
				}
			}
		}
		// find the closest enemy in the targets array that isn't blocked by a wall
		float minDist = Mathf.Infinity;
		if(targets.Count > 0) {
			Targetable newTarget = null;
			foreach(Targetable t in targets) {
				if(t.distanceFromPlayer < minDist) {
					RaycastHit hit;
					if(!Physics.Raycast(transform.position, t.transform.position - transform.position, out hit, t.distanceFromPlayer - t.radius, rayMask)) {
						minDist = t.distanceFromPlayer;
						newTarget = t;
					} else {
						//Debug.Log("Raycst hit: " + hit.transform.gameObject);
					}
				}
			}
			// if we're not in the middle of an attack or combo, update target
			if(!attacking) {
				target = newTarget;
				if(target is Enemy) ((Enemy)target).hpBar.gameObject.SetActive(true);
			}
		} else {
			target = null;
		}
	}

	public bool IsInRange(Targetable e) {
		if(!e.canTarget) return false;

		bool targetInFront = e.IsInFrontOf(this);
		bool lookingAtTarget = Helper.IsInFrontOf(GameController.mainCam.transform, e.transform);
		bool camInFrontOfPlayer = Helper.IsInFrontOf(transform, GameController.mainCam.transform);
		bool validPosition = lookingAtTarget ? (targetInFront ? true : camInFrontOfPlayer) : false;
		if(!validPosition) return false;

		float maxDist = 0.175f;  // only check objects in a circle of a radius of this fraction of the screen size
		e.SetScreenCoords();
		float dist = Vector2.Distance(e.screenCoords, screenCenter);
		if(dist < maxDist) {
			return true;
		}
		return false;
	}

	protected override void SetMotion() {
		bool inH = true, inV = true;    // used in conjunction to determine whether the player is doing any input
		if(rightKey != 0) {
			rightMov = Mathf.Lerp(rightMov, (rightKey * speed), accel);
		} else {
			inH = false;
			rightMov = Mathf.Lerp(rightMov, 0f, decel);
		}
		if(fwdKey != 0 || sprinting) {
			fwdMov = Mathf.Lerp(fwdMov, sprinting ? runSpeed : (fwdKey * speed), accel);
		} else {
			inV = false;
			fwdMov = Mathf.Lerp(fwdMov, 0f, decel);
		}
		anim.SetBool("input", inH || inV);
		SetVelocity();
	}

	protected override void SetVelocity() {
		// change forward's y to 0 then normalize, in case the camera is pointed down or up
		Vector3 tempForward = camTransform.forward;
		tempForward.y = 0f;

		// get movement direction vector
		if(sprinting && !attacking && (velocity != Vector3.zero || !isLightning)) {
			TurnIntoLightning(true);
			velocity = Vector3.Lerp(velocity, (tempForward.normalized * fwdMov + camTransform.right.normalized * rightMov * 10), 0.1f);
		} else {
			TurnIntoLightning(false);
			velocity = (tempForward.normalized * fwdMov + camTransform.right.normalized * rightMov);
		}
	}

	private void SetHandTrails(bool enabled) {
		foreach(TrailRenderer t in handTrails) {
			if(enabled) t.Clear();
			t.enabled = enabled;
		}
	}

	#region Abilities

	private void TurnIntoLightning(bool enable) {
		if(enable && !isLightning) {
			lightning.SetParticles(new ParticleSystem.Particle[0], 0);  // destroy any active particles
			renderer.enabled = false;   // make player disappear
			lightning.Play();       // start particles
			head.Play();
			transform.rotation = camTransform.rotation;
			cam.SetZoomTransform(sprintCam, 0.1f);
			burst.Play();
			cam.ScreenShake(1.5f);
			//flasher.FlashStart(Color.red, Color.white, -1);
			isLightning = true;     // protect this part from repeated calls
		} else if(!enable && isLightning) {
			renderer.enabled = true;    // make player reappear
			lightning.Stop();       // stop particles
			head.Stop();
			cam.SetZoomTransform(null);
			burst.Play();
			//flasher.FlashStop();
			if(rightKey <= 0.1f && fwdKey <= 0.1f) {
				anim.SetTrigger("land");
			}
			isLightning = false;    // protect this part from repeated calls
		}
	}

	private void Possess(Enemy e) {
		e.SetPlayer();
		TurnIntoLightning(false);
		renderer.enabled = false;
		possessing = true;
		gameObject.layer = LayerMask.NameToLayer("IgnoreCollision");
		targets.Remove(e);
		possessed = e;
		fwdMov = 0;		// instantly decelerate these so momentum doesn't carry
		rightMov = 0;	// over when we Unpossess
	}

	/**If the run key is pressed, we're manually un-possessing an enemy;
	 * otherwise, our control is being revoked, likely because it's dying.
	 */
	public void Unpossess(bool runKey) {
		possessing = false;
		gameObject.layer = LayerMask.NameToLayer("Characters");
		if(runKey) {
			//gameObject.layer = LayerMask.NameToLayer("IgnoreCollision");	// ignore collisions for 2 frames
			//StartCoroutine("EnableCollision");
			possessed.hpBar.gameObject.SetActive(false);
			possessed = null;
			SetPlayer();
			TurnIntoLightning(true);
			if(target != null) {	// don't use CanPossessTarget() here because you can freely jump to other enemies once you've possessed one
				control = state.AI;
			} else {
				control = state.PLAYER;
			}
		} else {
			isLightning = true;	// cheat to make TurnIntoLightning(false) work
			TurnIntoLightning(false);
			SetPlayer();
		}
	}

	protected override void Attack1() {
		base.Attack1();
		SetHandTrails(true);
		switch(comboNumber) {
			case 0:
				StartCoroutine("Attack1aCR");
				break;
			case 1:
				StartCoroutine("Attack1bCR");
				break;
			default:
				StartCoroutine("Attack1cCR");
				break;
		}
		comboNumber = (comboNumber + 1) % 3;
	}

	protected IEnumerator Attack1aCR() {
		// start animation, handle code in animation events
		// can just replace these coroutines
		yield return null;
	}

	protected IEnumerator Attack1bCR() {
		yield return null;
	}

	protected IEnumerator Attack1cCR() {
		yield return null;
	}

	protected override void Attack2() {
		SetHandTrails(true);
		base.Attack2();
		cooldownTimer = 1.6f;
		anim.SetTrigger("attack2Charge");
		anim.SetTrigger("attack2");
		StartCoroutine("Attack2CR");
	}

	private IEnumerator Attack2CR() {
		canSprint = false;
		yield return new WaitForSeconds(0.8f);
		a2fx.Play();//gameObject.SetActive(true);
		// exert hitbox if we decide to make it multi-hit
		if(target != null) target.Damage(25);
		if(target is Enemy) ((Enemy)target).Stun();
		else if(target is Door) {
			((Door)target).Open(true);
			((Door)target).Spark();
		}
		yield return new WaitForSeconds(0.8f);
		canSprint = true;
		//a2fx.Deactivate();
	}

	#endregion

	protected bool CanPossessTarget() {
		return target is Enemy && ((Enemy)target).control == state.STUNNED && canSprint;
	}

	//private IEnumerator EnableCollision() {
	//	yield return null;
	//	yield return null;
	//	yield return new WaitForSeconds(0.1f);
	//	gameObject.layer = LayerMask.NameToLayer("Characters");
	//}

}
