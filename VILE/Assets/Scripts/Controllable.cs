using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]	// NEEDED FOR COLLISION EVENTS
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class Controllable : Targetable {

	// NOTE: COLLISION DETECTION MUST BE CONTINUOUS or animations will be wacky
	#region Variables
	[SerializeField] protected float speed = 0.225f;
	[SerializeField] protected float acceleration = 0.175f;
	protected float accel {
		get {
			return acceleration * 60 * Time.deltaTime;
		}
	}
	[SerializeField] protected float gravity = 0.03f;
	protected float grav {
		get {
			return gravity * 60 * Time.deltaTime;
		}
	}
	[HideInInspector] public Vector3 yMove = Vector3.zero;	// need to keep vertical movement (this) separate from horizontal (velocity)
															// so it won't be affected by Lerps in SetVelocity()
	[SerializeField] protected float camDistance = 14;

	// hp-related
	[SerializeField] protected float maxHP = 100;
	protected float h = 100;
	protected float hp {
		get {
			return h;
		}
		set {
			h = Mathf.Clamp(value, 0, maxHP);
			hpBar.value = h;
			if(h == 0 && !dead) Die();
		}
	}
	[HideInInspector] public bool dead = false;
	public UIBar hpBar;

	// stamina and attacking
	protected bool attacking = false;
	protected float st = 100;
	public float stamina {
		get {
			return st;
		}
		set {
			st = Mathf.Clamp(value, 0, 100);
			if(this is Player) {
				((Player)this).stBar.value = st;
			}
		}
	}
	public float attack1Power = 10f;
	public float attack2Power = 20f;
	[SerializeField] protected float atk1Cost = 5;
	[SerializeField] protected float atk2Cost = 20;
	[SerializeField] protected float attack1Cooldown = 0.5f;
	[SerializeField] protected float attack2Cooldown = 3f;
	private float ct = 0;	// attack cooldowns (in seconds) - attacks fail while this > 0
	protected float cooldownTimer {
		get {
			return ct;
		}
		set {
			if(!suspendTimer) {
				ct = Mathf.Max(0, value);
				if(ct == 0) attacking = false;
			}
		}
	}
	protected bool suspendTimer = false;

	// velocity and transform-related
	[HideInInspector]
	public bool onGround = false;	// defaults to always true for characters without GroundTests
	[HideInInspector] public Vector3 velocity = Vector3.zero;	// direction and speed of attempted movement
	protected Vector3 objectVelocity = Vector3.zero;			// actual tracked velocity of the object
	protected bool onSlope = false;
	protected Vector3 prevPosition;
	protected Vector3 hitNormal = Vector3.zero;
	protected Quaternion playerRot = Quaternion.identity;
	protected Vector3 motionInput = Vector3.zero;

	// input
	[HideInInspector] public bool readInput = true;
	protected bool atk1Key = false;
	protected bool atk2Key = false;

	// miscellaneous
	protected CharacterController cc;
	protected Rigidbody rb;
	protected new Renderer renderer;
	[HideInInspector] public Animator anim;
	protected int stunCount = 0;
	protected float prevAnimSpeed = 1;  // for stopping animation when stunned, then resuming
	protected Vector3 newHpScale = Vector3.one, newStScale = Vector3.one;
	protected float defaultGracePeriod = 0.8f;
	[HideInInspector] public bool invincible = false;
	protected IEnumerator gracePeriodCR;
	[Tooltip("How far this Controllable can see")]
	public float sightLength = 200;
	[HideInInspector] public state control = state.AI;
	/**Camera is not affected by the target.
	 * The target only affects where you face when you're attacking.
	 * target can only change when you're not attacking.
	 * target updates every non-attacking frame to the enemy nearest to the center of the screen.
	 */

	protected static Controllable currentPlayer;
	[HideInInspector] public Targetable target;

	public enum state {
		AI, PLAYER, STUNNED
	};
	#endregion

	protected override void Reset() {
		base.Reset();

		rb = GetComponent<Rigidbody>();
		rb.useGravity = false;
		rb.isKinematic = true;
		cc = GetComponent<CharacterController>();
		anim = GetComponentInChildren<Animator>();

		renderer = GetComponentInChildren<SkinnedMeshRenderer>();
		if(renderer == null) renderer = GetComponentInChildren<MeshRenderer>();

		h = maxHP;
	}
	
	protected virtual void Start() {
		Reset();
	}
	
	protected override void Update() {
		base.Update();
		if(control == state.PLAYER) {
			PlayerUpdate();
		} else if(control == state.AI) {
			AIUpdate();
		} else if(control == state.STUNNED) {
			// stun effects
			ApplyGravity();
			cc.Move(yMove);
		}
		cooldownTimer -= Time.deltaTime;
	}

	protected virtual void FixedUpdate() {
		if(control == state.PLAYER) {
			PlayerFixedUpdate();
		} else {
			AIFixedUpdate();
		}
	}

	// Handle sliding while falling and head-hitting while jumping
	protected virtual void OnControllerColliderHit(ControllerColliderHit hit) {
		hitNormal = hit.normal;
		// notOnSlope = we're on ground level enough to walk on OR we're hitting a straight-up wall
		onSlope = !(Vector3.Angle(Vector3.up, hitNormal) <= cc.slopeLimit || Vector3.Angle(Vector3.up, hitNormal) >= 89);
		if(hit.point.y > transform.position.y + 4f && Mathf.Sqrt(Mathf.Pow(transform.position.x - hit.point.x, 2f) + Mathf.Pow(transform.position.z - hit.point.z, 2f)) < 2f * transform.localScale.x) {
			// hit something going up
			yMove.y = Mathf.Min(0f, velocity.y);
		}
	}

	#region Update methods

	protected virtual void PlayerUpdate() {
		if(!attacking) {
			SetControls();
			SetVelocity();
			PlayerAttack();
		}
		CommonUpdate();
		// (attacking can change in PlayerAttack())
		if(attacking) {
			PlayerDirection();
		}
	}

	// Move character based on velocity; handle some additional physics
	protected virtual void CommonUpdate() {
		ApplyGravity();

		// sliding
		float slideFriction = 0.5f;
		if(onSlope) {
			velocity.x += -velocity.y * hitNormal.x * (1f - slideFriction);
			velocity.z += -velocity.y * hitNormal.z * (1f - slideFriction);
		}

		// motion & velocity tracking
		cc.Move((velocity + yMove) * 60 * Time.smoothDeltaTime);
		objectVelocity = transform.position - prevPosition;
		anim.SetFloat("speed", objectVelocity.magnitude);
		Vector3 tempForward = objectVelocity.magnitude <= 0.01f ? transform.forward : objectVelocity;
		tempForward.y = 0;
		tempForward.Normalize();
		transform.forward = Vector3.Slerp(transform.forward, tempForward, 0.2f);
		prevPosition = transform.position;
	}
	// Set player's direction while attacking based on target
	protected virtual void PlayerDirection() {
		// While attacking, turn to face the enemy. If there is no enemy, face camera's forward
		if(target != null) {
			target.SetScreenCoords();  // make the reticle keep moving
			Vector3 toTarget = target.transform.position - transform.position;
			toTarget.y = 0;
			toTarget = toTarget.normalized;
			transform.forward = Vector3.Slerp(transform.forward, toTarget, 0.2f);
		} else {
			Vector3 newForward = GameController.mainCam.transform.forward;
			newForward.y = 0;
			transform.forward = Vector3.Slerp(transform.forward, newForward, 0.2f);
		}
	}
	// React to attack input
	protected virtual void PlayerAttack() {
		if(atk1Key && CanAttack(atk1Cost)) Attack1();
		else if(atk2Key && CanAttack(atk2Cost)) Attack2();
	}
	protected virtual void ApplyGravity() {
		if(!onGround) {
			yMove.y -= grav;
		}
		// yMove is reset to 0 in GroundTest upon land (doing it here every frame would mess up force exertion)
	}

	protected virtual void AIUpdate() {
		// This is just here to apply gravity. Velocity will be whatever it's left at.
		CommonUpdate();
	}

	protected virtual void PlayerFixedUpdate() {

	}

	protected virtual void AIFixedUpdate() {

	}

	#endregion

	protected virtual void SetTarget() {

	}

	#region Setting control input variables

	// Accept all input
	protected virtual void SetControls() {
		if(readInput) {
			SetMovementKeys();
			SetAttackControls();
		}
	}

	// Accept movement input (rightKey, fwdKey)
	protected virtual void SetMovementKeys() {
		motionInput.x = Input.GetAxisRaw("Horizontal");
		motionInput.z = Input.GetAxisRaw("Vertical");
	}

	// Accept attack input (atk1Key, atk2Key)
	protected virtual void SetAttackControls() {
		atk1Key = Input.GetButtonDown("Attack1");
		atk2Key = Input.GetButtonDown("Attack2");
	}

	#endregion

	// Set velocity based on key inputs
	protected virtual void SetVelocity() {
		Vector3 tempForward;
		if(motionInput.magnitude == 0) {
			anim.SetBool("input", false);
			tempForward = transform.forward;    // if no keys are held, keep going the way we were (decelerate)
		} else {
			anim.SetBool("input", true);
			tempForward = GameController.mainCam.transform.forward; // else go in the direction the camera points
		}
		tempForward.y = 0f;
		tempForward.Normalize();

		velocity = Vector3.Lerp(
			velocity, 
			(tempForward * motionInput.normalized.z + GameController.mainCam.transform.right * motionInput.normalized.x) * speed, 
			accel * (onGround ? 1 : 0.1f));
	}

	// Make this object the player
	public virtual void SetPlayer() {
		// revoke control from current controlled object
		if(currentPlayer != null) {
			currentPlayer.control = state.AI;
			currentPlayer.SetTarget();
			if(currentPlayer is Player) {
				// disable player's CharacterController to prevent multiple Possess calls because
				// that breaks things
				currentPlayer.cc.enabled = false;
			}
		}

		// give control to this object
		Unstun();
		control = state.PLAYER;
		GameController.camControl.idistance = camDistance;
		GameController.camControl.lookAt = camLook;
		cooldownTimer = 0;
		if(cc != null) cc.enabled = true;
		currentPlayer = this;
	}

	public virtual void Stun() {
		control = state.STUNNED;
		velocity = Vector3.zero;
		stunCount++;
		anim.speed = 0;
		StopCoroutine("UnstunCR");
		StartCoroutine("UnstunCR");
	}

	protected virtual IEnumerator UnstunCR() {
		yield return new WaitForSeconds(4);
		Unstun();
	}

	public virtual void Unstun() {
		StopCoroutine("UnstunCR");
		control = state.AI;
		anim.speed = prevAnimSpeed;
	}

	protected virtual void Attack1() {
		attacking = true;
		stamina -= atk1Cost;
		cooldownTimer = attack1Cooldown;
	}

	protected virtual void Attack2() {
		attacking = true;
		stamina -= atk2Cost;
		cooldownTimer = attack2Cooldown;
	}

	// Check if we can attack using a given amount of stamina
	protected virtual bool CanAttack(float atkCost) {
		return onGround && stamina >= atkCost && cooldownTimer <= 0;
	}

	public virtual void Damage(float damage) {
		Damage(damage, defaultGracePeriod);
	}

	public virtual void Damage(float damage, float gracePeriod) {
		if(!invincible) {
			DeductHP(damage);
			float powerFactor = damage / 120;
			float deathFactor = dead ? 5 : 1;
			if(powerFactor * deathFactor > 0.02f) {
				GameController.HitStop(Mathf.Min(0.8f, powerFactor * deathFactor));
			}
			gracePeriodCR = GracePeriod(gracePeriod);
			StartCoroutine(gracePeriodCR);
		}
	}

	// can be overridden, e.g. to implement location-based damage modifiers
	public virtual void DeductHP(float damage) {
		hp -= damage;
	}

	public virtual void Knockback(Vector3 force) {
		anim.SetTrigger("hurt");
	}

	protected virtual IEnumerator GracePeriod(float gracePeriod) {
		invincible = true;
		yield return new WaitForSeconds(gracePeriod);
		invincible = false;
	}

	protected virtual void Die() {
		dead = true;
		//gameObject.SetActive(false);
		hpBar.gameObject.SetActive(false);
		//Destroy(gameObject);	// replace this when we have overrides
	}
}
