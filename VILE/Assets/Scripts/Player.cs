using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Controllable {
	[SerializeField] protected float runSpeed = 0.2f;
	// claws are needed for animations
	[SerializeField] private float clawLThickness = 0, clawRThickness = 0;
	[SerializeField] private bool clawLVisible = false, clawRVisible = false;
	[SerializeField] private TessClaw clawL, clawR;	// these are used

	private ParticleSystem lightning;
	private ParticleSystem burst;
	private ParticleSystem head;
	[HideInInspector] public bool isLightning = false;
	private Transform sprintCam;
	private EpilepsyController flasher;
	private static Vector2 screenCenter = new Vector2(0.5f, 0.5f);
	//private LightningMeshEffect a2fx;
	private ParticleSystem a2fx;
	private bool canPossess = true;
	private LayerMask rayMask;
	private LayerMask solidLayer;
	private AttackHitbox ahbL, ahbR;
	public UIBar stBar;
	private float rechargeFactor = 0.05f;
	protected bool sprintKey = false;
	protected bool sprinting = false;
	[HideInInspector] public bool stomping = false;
	protected Vector3 stompVelocity = new Vector3(0, -5, 0);
	protected bool disableSprint = false;
	protected bool stompEnabled = false;

	[HideInInspector] public bool possessing = false;
	[HideInInspector] public Enemy possessed = null;
	[HideInInspector] public Vector3 iPos = Vector3.zero;

	// debug
	//public Vector3 targetRayHitPoint, targetRayStart, targetRayEnd;

	public static List<Targetable> targets = new List<Targetable>();

	protected override void Reset() {
		base.Reset();
		lightning = GetComponentsInChildren<ParticleSystem>()[0];
		burst = GetComponentsInChildren<ParticleSystem>()[1];
		head = GetComponentsInChildren<ParticleSystem>()[2];
		sprintCam = GameObject.Find("SprintCam").transform;
		flasher = FindObjectOfType<EpilepsyController>();
		a2fx = GetComponentsInChildren<ParticleSystem>()[3];
		solidLayer = LayerMask.NameToLayer("Solid");
		rayMask = 1 << LayerMask.NameToLayer("Solid") | 1 << LayerMask.NameToLayer("Enemies") | 1 << LayerMask.NameToLayer("FlyingCharacters") | 1 << LayerMask.NameToLayer("Default");
		hpBar = GameObject.Find("Tess HP").GetComponent<UIBar>();
		hpBar.character = this;
		hpBar.maxValue = maxHP;
		stBar = GameObject.Find("Tess St").GetComponent<UIBar>();
		stBar.character = this;
		stBar.maxValue = 100;
	}

	protected override void Start() {
		base.Start();
		
		prevPosition = transform.position;
		GameController.mainCam.transform.position = transform.position;

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		SetPlayer();
	}

	protected override void Update() {
		// check if we fell off the map
		if(transform.position.y < -1) transform.position = iPos;

		if(!possessing) {
			// normal, non-possessing update method
			base.Update();
			anim.SetBool("attacking", attacking);
		} else {
			// follow enemy we're possessing
			transform.position = possessed.transform.position;
			transform.forward = GameController.mainCam.transform.forward;

			// continue setting target
			SetTarget();

			// accept unpossess input
			if(Input.GetButtonDown("Run")) {
				Unpossess(true);
			}
		}
		#region claw animations
		clawL.gameObject.SetActive(clawLVisible);
		if(clawLVisible) {
			clawL.thicknessRandomness = clawLThickness;
			foreach(TrailRenderer t in clawL.trails) {
				t.widthMultiplier = clawLThickness * 2;
			}
		}
		clawR.gameObject.SetActive(clawRVisible);
		if(clawRVisible) {
			clawR.thicknessRandomness = clawRThickness;
			foreach(TrailRenderer t in clawR.trails) {
				t.widthMultiplier = clawRThickness * 2;
			}
		}
		#endregion
	}

	protected override void PlayerUpdate() {
		SetControls();
		// Set the target normally if we're not attacking.
		// If we're attacking, only set the target when we try to move forward.
		if(!attacking) {
			SetVelocity();
			SetTarget();
			if(atk2Key && CanAttack(atk2Cost)) Attack2();
		}

		// debug
		if(Input.GetButtonDown("Jump")) {
			yMove.y = 1;
			StartCoroutine("EnableStomp");
		}

		CommonUpdate();
		if(!attacking || anim.GetBool("attackComboing")) {
			if(atk1Key && CanAttack(atk1Cost)) Attack1();
		}
		if(attacking) {
			PlayerDirection();
			if(target != null) {
				a2fx.transform.LookAt(target.camLook);
			} else a2fx.transform.forward = GameController.mainCam.transform.forward;
			if(motionInput.z > 0) {
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
		} else {
			cc.Move((target.transform.position - transform.position).normalized * runSpeed * 1.5f * 60 * Time.smoothDeltaTime);
		}
	}

	protected override void OnControllerColliderHit(ControllerColliderHit hit) {
		base.OnControllerColliderHit(hit);
		// if we hit something under us
		if(hit.gameObject.layer == LayerMask.NameToLayer("Solid") && hit.point.y - transform.position.y < 0.5f) {
			if(stomping) {
				stomping = false;
				stamina -= 15;
				GameController.camControl.ScreenShake(2);
				anim.SetTrigger("stomp");
			}
		}
		if(hit.gameObject.GetComponent<Enemy>() == target && control == state.AI) {
			//Debug.Log("Possessing " + target);
			Possess((Enemy)target);
		} else if(hit.gameObject.GetComponent<Conductor>() == target && control == state.AI) {
			TurnIntoLightning(false);
			renderer.enabled = false;
			GameController.mainCam.ScreenShake(1);
			GameController.camControl.lookAt = target.transform;
		}
		// make a "checkpoint" to keep from falling off map
		if(onGround && Mathf.Floor(Time.time) % 2 == 0) iPos = transform.position;
	}

	#region Targeting

	/**Set the target to the closest targetable in the targets array
	 */
	protected override void SetTarget() {

		/* NOTE: The below line disables the target's HPBar, and reenables it later if it's still the target.
		 * However, UIBar.Update() is called at some point IN BETWEEN those two lines. As a fix, UIBar.Update() 
		 * was moved to UIBar.LateUpdate().
		 */
		if(target is Enemy) ((Enemy)target).hpBar.gameObject.SetActive(false);
		// add any onscreen targets that are close enough to the center of the screen
		// to the targets array (and remove those who aren't)
		foreach(Targetable t in onScreen) {
			bool inRange = IsInRange(t);
			if(t != possessed) {
				if(!t.isTarget && inRange) {
					t.isTarget = true;
					targets.Add(t);
				} else if(t.isTarget && !inRange) {
					t.isTarget = false;
					targets.Remove(t);
				}
			}
		}
		// find the closest targetable in the targets array that isn't blocked by a wall
		float minDist = Mathf.Infinity;
		if(targets.Count > 0) {
			Targetable newTarget = null;
			foreach(Targetable t in targets) {
				float distScore = t.distanceFromPlayerSquared + Mathf.Pow(t.distanceFromCenter * 100, 2);
				if(distScore < minDist) {
					RaycastHit hit;
					if(Physics.Linecast(camLook.position, t.camLook.position, out hit, rayMask)) {
						if(Helper.GetRelatedTargetable(hit.collider.gameObject) == t) {
							minDist = distScore;
							newTarget = t;
						}
						//targetRayHitPoint = hit.point;
					} else {
						// If this linecast fails when it shouldn't, the enemy character's collider is probably too small
					}
				}
			}
			target = newTarget;
			if(target is Enemy) ((Enemy)target).hpBar.gameObject.SetActive(true);
		} else {
			target = null;
		}
	}

	// debug
	//private void OnDrawGizmos() {
	//	Gizmos.color = Color.red;
	//	foreach(Targetable t in onScreen) {
	//		if(IsInRange(t)) Gizmos.color = Color.red;
	//		else Gizmos.color = Color.green;
	//		Gizmos.DrawLine(camLook.position, t.camLook.position);
	//		//Gizmos.DrawWireSphere(t.camLook.position, t.radius);
	//	}
	//	Gizmos.color = Color.yellow;
	//	Gizmos.DrawSphere(targetRayHitPoint, 1f);
	//}

	public bool IsInRange(Targetable e) {

		if(!e.canTarget) return false;
		if(e.distanceFromPlayerSquared > sightLength * sightLength) return false;

		bool targetInFront = e.IsInFrontOf(this);
		bool lookingAtTarget = Helper.IsInFrontOf(GameController.mainCam.transform, e.transform);
		bool camInFrontOfPlayer = Helper.IsInFrontOf(transform, GameController.mainCam.transform);
		bool validPosition = lookingAtTarget ? (targetInFront ? true : camInFrontOfPlayer) : false;
		if(!validPosition) return false;

		float maxDist = 0.175f;  // only check objects in a circle of a radius of this fraction of the screen size
		e.SetScreenCoords();
		e.distanceFromCenter = Vector2.Distance(e.screenCoords, screenCenter);
		if(e.distanceFromCenter < maxDist) {
			return true;
		}
		return false;
	}

	#endregion

	#region Controls & velocity

	protected override void SetControls() {
		base.SetControls();
		SetSprintKey();
	}

	// Accept sprint input (sprintKey)
	protected virtual void SetSprintKey() {
		sprintKey = Input.GetButton("Run");
		if(Input.GetButtonUp("Run")) {
			disableSprint = false;
		}
	}

	protected override void SetVelocity() {
		sprinting = stamina > 0 && !disableSprint ? sprintKey : false;

		anim.SetBool("input", motionInput.magnitude != 0);

		Vector3 tempForward = GameController.mainCam.transform.forward;
		tempForward.y = 0f;
		tempForward.Normalize();

		// get velocity
		if(stomping) {
			velocity = stompVelocity;
		} else if(sprinting && !attacking && (calculatedVelocity.magnitude != 0 || !isLightning)) {
			// sprinting

			// stomp check
			if(motionInput.z < 0 && !onGround && stompEnabled && stamina > 15) {
				stomping = true;
				disableSprint = true;	// to keep us from instantly sprinting again when we hit the ground in
										//		the case that we're still holding the sprint key
				readInput = false;      // to keep us from moving for a bit while the stomp animation finishes
				ResetControls();		// to keep leftover input from making us move while reading input is disabled
			} else {
				TurnIntoLightning(true);
				velocity = Vector3.Lerp(velocity,
					(tempForward + GameController.mainCam.transform.right.normalized * motionInput.x * 1.5f).normalized * runSpeed,
					0.1f * 60 * Time.deltaTime);
			}
		} else {
			// not sprinting
			TurnIntoLightning(false);
			velocity = Vector3.Lerp(
				velocity,
				(tempForward * motionInput.normalized.z + GameController.mainCam.transform.right * motionInput.normalized.x) * speed,
				accel * (onGround ? 1 : 0.1f));
		}
	}

	#endregion

	#region Abilities

	private void TurnIntoLightning(bool enable) {
		if(enable && !isLightning) {
			lightning.SetParticles(new ParticleSystem.Particle[0], 0);  // destroy any active particles
			renderer.enabled = false;   // make player disappear
			lightning.Play();       // start particles
			head.Play();
			transform.rotation = GameController.mainCam.transform.rotation;
			GameController.camControl.SetZoomTransform(sprintCam, 0.1f);
			burst.Play();
			GameController.camControl.ScreenShake(1.5f);
			if(gracePeriodCR != null) StopCoroutine(gracePeriodCR);
			invincible = true;
			//anim.speed = 0;
			//flasher.FlashStart(Color.red, Color.white, -1);
			isLightning = true;     // protect this part from repeated calls
		} else if(!enable && isLightning) {
			renderer.enabled = true;    // make player reappear
			lightning.Stop();       // stop particles
			head.Stop();
			GameController.camControl.SetZoomTransform(null);
			burst.Play();
			invincible = false;
			StopCoroutine("EnableStomp");
			stompEnabled = false;
			//anim.speed = 1;
			//flasher.FlashStop();
			if(motionInput.magnitude < 0.1f && onGround) {
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
		//fwdMov = 0;     // instantly decelerate these so momentum doesn't carry
		//rightMov = 0;	// over when we Unpossess
		anim.speed = 0;
	}

	/**If the run key is pressed, we're manually un-possessing an enemy;
	 * otherwise, our control is being revoked, likely because it's dying.
	 */
	public void Unpossess(bool runKey) {
		possessing = false;
		gameObject.layer = LayerMask.NameToLayer("Characters");
		anim.speed = 1;
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
		velocity = Vector3.zero;
		anim.SetTrigger("attack1");
	}

	public void AnimFunc_DrainStamina() {
		stamina -= atk1Cost;
	}

	public void AnimFunc_UnsetComboing() {
		anim.SetBool("attackComboing", false);
		attacking = false;
		suspendTimer = false;
	}

	public void AnimFunc_SetComboing() {
		anim.SetBool("attackComboing", true);
		attacking = true;
		suspendTimer = true;
	}

	// in case we somehow land while performing an attack, we don't want to transition to land
	public void AnimFunc_UnsetLand() {
		anim.ResetTrigger("land");
	}

	public void AnimFunc_OnHurtEnd() {
		hurtAnimPlaying = false;
	}

	public void AnimFunc_OnStompEnd() {
		readInput = true;
	}

	protected override void Attack2() {
		base.Attack2();
		velocity = Vector3.zero;
		cooldownTimer = attack2Cooldown;
		anim.SetTrigger("attack2Charge");
		anim.SetTrigger("attack2");
		StartCoroutine("Attack2CR");
	}

	private IEnumerator Attack2CR() {
		canPossess = false;
		yield return new WaitForSeconds(0.8f);
		a2fx.Play();
		yield return new WaitForSeconds(0.4f);
		// exert hitbox, don't stun enemies (only target)
		if(target is Enemy) {
			((Enemy)target).Damage(attack2Power);
			((Enemy)target).Stun();
		} else if(target is Door) {
			((Door)target).Open(true);
			((Door)target).Spark();
		}
		yield return new WaitForSeconds(attack2Cooldown - 1.2f);
		canPossess = true;
	}

	#endregion

	public override void Damage(float damage, float gracePeriod) {
		if(!invincible) {
			GameController.camControl.ScreenShake(damage / 100 * 2);
		}
		base.Damage(damage, gracePeriod);
		if(dead) {
			GameController.HitStop(1);
		}
	}

	public bool CanPossessTarget() {
		return canPossess && (target is Conductor || target is Enemy && ((Enemy)target).control == state.STUNNED);
	}

	private IEnumerator EnableStomp() {
		Debug.Log("Enabling stomp");
		yield return new WaitForSeconds(0.5f);
		Debug.Log("Stomp enabled");
		stompEnabled = true;
	}
}
