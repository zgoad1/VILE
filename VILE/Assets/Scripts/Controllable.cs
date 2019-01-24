using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class Controllable : Targetable {

	// NOTE: COLLISION DETECTION MUST BE CONTINUOUS or animations will be wacky
	#region Variables
	[SerializeField] protected float speed = 0.225f;
	[SerializeField] protected float runSpeed = 0.2f;
	[SerializeField] protected float acceleration = 0.175f;
	protected float accel {
		get {
			return acceleration * 60 * Time.deltaTime;
		}
	}
	[SerializeField] protected float deceleration = 0.2f;
	protected float decel {
		get {
			return deceleration * 60 * Time.deltaTime;
		}
	}
	[SerializeField] protected float gravity = 0.03f;
	protected float grav {
		get {
			return gravity * 60 * Time.deltaTime;
		}
	}
	protected Vector3 gravVec = Vector3.zero;
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
	public bool dead = false;
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
	private bool og = false;
	[HideInInspector] public bool onGround {
		get {
			return og;
		}
		set {
			og = value;
		}
	}
	[HideInInspector] public Vector3 velocity = Vector3.zero;
	protected float rightMov = 0f;
	protected float fwdMov = 0f;
	protected float upMov = 0f;
	protected bool sprinting = false;
	protected bool notOnSlope = false;
	protected Vector3 prevPosition;
	protected Vector3 hitNormal = Vector3.zero;
	protected Quaternion playerRot = Quaternion.identity;

	// input
	[HideInInspector] public bool readInput = true;
	protected float rightKey;
	protected float fwdKey;
	protected bool sprintKey = false;
	protected bool atk1Key = false;
	protected bool atk2Key = false;

	// miscellaneous
	public static Camera mainCam;
	protected Transform camTransform;
	protected CameraControl cam;
	protected CharacterController cc;
	protected Rigidbody rb;
	protected new Renderer renderer;
	[HideInInspector] public Animator anim;
	protected int stunCount = 0;
	protected float prevAnimSpeed = 1;  // for stopping animation when stunned, then resuming
	protected Vector3 newHpScale = Vector3.one, newStScale = Vector3.one;
	protected float defaultGracePeriod = 0.8f;
	[HideInInspector] public bool invincible = false;

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

		mainCam = FindObjectOfType<MainCamera>().GetComponent<Camera>();
		camTransform = FindObjectOfType<MainCamera>().transform;
		cam = FindObjectOfType<CameraControl>();
		cc = GetComponent<CharacterController>();
		anim = GetComponentInChildren<Animator>();

		rb = GetComponent<Rigidbody>();
		rb.useGravity = false;
		rb.isKinematic = true;

		renderer = GetComponentInChildren<SkinnedMeshRenderer>();
		if(renderer == null) renderer = GetComponentInChildren<MeshRenderer>();

		gravVec = new Vector3(0, -grav, 0);

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
			velocity += gravVec;
			cc.Move(velocity);
		}
		cooldownTimer -= Time.deltaTime;
		anim.SetBool("attacking", attacking);
	}

	protected virtual void FixedUpdate() {
		if(control == state.PLAYER) {
			PlayerFixedUpdate();
		} else {
			AIFixedUpdate();
		}
	}

	protected virtual void OnControllerColliderHit(ControllerColliderHit hit) {
		if(hit.gameObject.tag != "Transparent") {
			hitNormal = hit.normal;
			// notOnSlope = we're on ground level enough to walk on OR we're hitting a straight-up wall
			notOnSlope = Vector3.Angle(Vector3.up, hitNormal) <= cc.slopeLimit || Vector3.Angle(Vector3.up, hitNormal) >= 89;
			if(velocity.y <= 0 && hit.point.y < transform.position.y + .9f) {
				// hit ground
				if(notOnSlope) onGround = true;
				//Debug.Log("Hit ground");
				// else if the hit point is from above and inside our radius (on top of head rather than on outer edge)
			} else if(hit.point.y > transform.position.y + 4f && Mathf.Sqrt(Mathf.Pow(transform.position.x - hit.point.x, 2f) + Mathf.Pow(transform.position.z - hit.point.z, 2f)) < 2f * transform.localScale.x) {
				// hit something going up
				upMov = Mathf.Min(0f, upMov);
				//Debug.Log("I hit my head!");
			}
		}
	}

	#region Update methods

	protected virtual void PlayerUpdate() {
		SetControls();
		sprinting = sprintKey;  // later change this to account for stamina
		if(!attacking) {
			PlayerMove();
			PlayerAttack();
		}
		// (attacking can change in PlayerAttack())
		if(attacking) {
			PlayerDirection();
		}
	}

	protected virtual void PlayerMove() {
		#region Calculate movement with boring math
		onGround = false;

		// calculate movement
		velocity.y = upMov;
		#region Sliding
		float slideFriction = 0.5f;
		if(!notOnSlope) {
			velocity.x += -upMov * hitNormal.x * (1f - slideFriction);
			velocity.z += -upMov * hitNormal.z * (1f - slideFriction);
			hitNormal = Vector3.zero;
			onGround = false;
			//Debug.Log("Sliding");
		}
		#endregion
		cc.Move(velocity * 60 * Time.smoothDeltaTime);  // T R I G G E R S   C O L L I S I O N   D E T E C T I O N  (AND CAN SET ONGROUND TO TRUE)

		// speed is the distance from where we were last frame to where we are now
		float movDist = Vector3.Distance(prevPosition, transform.position);
		anim.SetFloat("speed", movDist);
		velocity = (transform.position - prevPosition).normalized;
		velocity.y = 0;
		prevPosition = transform.position;
		//Debug.Log("speed: " + anim.GetFloat("speed") + "\nmovDirec: " + movDirec);
		transform.forward = Vector3.Slerp(transform.forward, velocity, 0.2f);
		playerRot.y = transform.rotation.y;
		playerRot.w = transform.rotation.w;
		transform.rotation = playerRot;

		// jumping & falling
		if(!onGround || !notOnSlope) {
			upMov -= grav;
		} else {
			upMov = -grav;
		}
		if(sprinting) {
			// sprinting is like a boost rather than an increase in movement speed.
			// i.e., instead of moving your legs faster, it's like attaching a rocket to your behind.
			// so if we're sprinting then we don't reset the movement direction.
		} else {
			velocity.x = 0f;
			velocity.z = 0f;
		}
		#endregion
	}
	protected virtual void PlayerDirection() {
		// While attacking, turn to face the enemy. If there is no enemy, face camera's forward
		if(target != null) {
			target.SetScreenCoords();  // make the reticle keep moving
			Vector3 toTarget = target.transform.position - transform.position;
			toTarget.y = 0;
			toTarget = toTarget.normalized;
			transform.forward = Vector3.Slerp(transform.forward, toTarget, 0.2f);
		} else {
			Vector3 newForward = camTransform.forward;
			newForward.y = 0;
			transform.forward = Vector3.Slerp(transform.forward, newForward, 0.2f);
		}
	}
	protected virtual void PlayerAttack() {
		if(atk1Key && CanAttack(atk1Cost)) Attack1();
		else if(atk2Key && CanAttack(atk2Cost)) Attack2();
	}

	protected virtual void AIUpdate() {

	}

	protected virtual void PlayerFixedUpdate() {

	}

	protected virtual void AIFixedUpdate() {

	}

	#endregion

	protected virtual void SetTarget() {

	}

	#region Setting control input variables

	protected virtual void SetControls() {
		if(readInput) {
			SetMovementKeys();
			SetSprintKey();
			SetAttackControls();
		}
	}

	protected virtual void SetMovementKeys() {
		rightKey = Input.GetAxisRaw("Horizontal");
		fwdKey = Input.GetAxisRaw("Vertical");
		if(Mathf.Abs(rightKey) == 1 && Mathf.Abs(fwdKey) == 1) {    // circular movement instead of square
			rightKey = Mathf.Sign(rightKey) * 0.707f;
			fwdKey = Mathf.Sign(fwdKey) * 0.707f;
		}
	}

	protected virtual void SetSprintKey() {
		sprintKey = Input.GetButton("Run");
	}

	protected virtual void SetAttackControls() {
		atk1Key = Input.GetButtonDown("Attack1");
		atk2Key = Input.GetButtonDown("Attack2");
	}

	#endregion

	// Set forward and right motion based on key inputs
	protected virtual void SetMotion() {
		bool inH = true, inV = true;    // used in conjunction to determine whether the player is doing any input
		if(rightKey != 0) {
			rightMov = rightKey * speed;
		} else {
			inH = false;
			rightMov = Mathf.Lerp(rightMov, 0f, decel);
		}
		if(fwdKey != 0 || sprinting) {
			fwdMov = fwdKey * speed;
		} else {
			inV = false;
			fwdMov = Mathf.Lerp(fwdMov, 0f, decel);
		}
		anim.SetBool("input", inH || inV);
		SetVelocity();
		//anim.SetFloat("speed", movDirec.magnitude);
	}

	// How to set moveDirec once we have fwdMov and rightMov
	// Override to implement sprinting, etc.
	protected virtual void SetVelocity() {
		// change forward's y to 0 then normalize, in case the camera is pointed down or up
		Vector3 tempForward;
		if(rightKey == 0 && fwdKey == 0) {
			tempForward = transform.forward;	// if no keys are held, keep going the way we were (decelerate)
		} else {
			tempForward = camTransform.forward;	// else go in the direction the camera points
		}
		tempForward.y = 0f;
		velocity = Vector3.Lerp(velocity, tempForward.normalized * fwdMov + camTransform.right.normalized * rightMov, accel);
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
		cam.idistance = camDistance;
		cam.lookAt = camLook;
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
		return stamina >= atkCost && cooldownTimer <= 0;
	}

	public virtual void Damage(float damage) {
		Damage(damage, defaultGracePeriod);
	}

	public virtual void Damage(float damage, float gracePeriod) {
		if(!invincible) {
			hp -= damage;
			float powerFactor = damage / 120;
			float deathFactor = dead ? 5 : 1;
			GameController.HitStop(Mathf.Min(0.8f, powerFactor * deathFactor));
			IEnumerator gp = GracePeriod(gracePeriod);
			StartCoroutine(gp);
		}
	}

	public virtual void Knockback(Vector3 force) {
		anim.SetTrigger("hurt");
	}

	protected IEnumerator GracePeriod() {
		invincible = true;
		yield return new WaitForSeconds(defaultGracePeriod);
		invincible = false;
	}

	protected IEnumerator GracePeriod(float gracePeriod) {
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
