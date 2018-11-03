using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Rigidbody))]
public class Controllable : MonoBehaviour {

	// NOTE: COLLISION DETECTION MUST BE CONTINUOUS or animations will be wacky
	#region Variables
	[SerializeField] protected float speed = 0.225f;
	[SerializeField] protected float runSpeed = 2f;
	[SerializeField] protected float accel = 0.175f;
	[SerializeField] protected float decel = 0.2f;
	[SerializeField] protected float grav = 0.03f;
	[SerializeField] protected float camDistance = 14;

	private bool og = false;    // whether Percy can jump
	[HideInInspector] public bool onGround {
		get {
			return og;
		}
		set {
			og = value;
		}
	}
	[HideInInspector] public Vector3 velocity = Vector3.zero;    // direction of movement
	public static Camera mainCam;

	protected Transform camTransform;
	protected CameraControl cam;
	protected CharacterController cc;
	protected float rightKey;
	protected float fwdKey;
	protected float rightMov = 0f;
	protected float fwdMov = 0f;
	protected float upMov = 0f;
	protected bool sprintKey = false;
	protected bool sprinting = false;
	protected Quaternion playerRot = Quaternion.identity;
	protected Vector3 hitNormal = Vector3.zero;
	protected bool notOnSlope = false;
	[HideInInspector] public bool readInput = true;
	protected Vector3 prevPosition;
	protected Animator anim;
	protected Rigidbody rb;
	protected new Renderer renderer;
	[HideInInspector] public state control = state.AI;
	/**Camera is not affected by the target.
	 * The target only affects where you face when you're attacking.
	 * target can only change when you're not attacking.
	 * target updates every non-attacking frame to the enemy nearest to the center of the screen.
	 */
	protected Transform camLook;

	protected static Controllable currentPlayer;
	[HideInInspector] public Controllable target;

	public enum state {
		AI, PLAYER
	};
	#endregion

	protected virtual void Reset() {
		mainCam = FindObjectOfType<MainCamera>().GetComponent<Camera>();
		camTransform = FindObjectOfType<MainCamera>().transform;
		cam = FindObjectOfType<CameraControl>();
		cc = GetComponent<CharacterController>();
		anim = GetComponentInChildren<Animator>();
		rb = GetComponent<Rigidbody>();
		rb.useGravity = false;
		rb.isKinematic = true;
		camLook = GetComponentInChildren<CamLookat>().transform;
		renderer = GetComponentInChildren<SkinnedMeshRenderer>();
		if(renderer == null) renderer = GetComponentInChildren<MeshRenderer>();
		if(renderer != null) {
			ControllableRenderer cr;
			if(this is Enemy) cr = renderer.GetComponent<EnemyRenderer>();
			else cr = renderer.GetComponent<ControllableRenderer>();
			if(cr == null) {
				if(this is Enemy) cr = renderer.gameObject.AddComponent<EnemyRenderer>();
				else cr = renderer.gameObject.AddComponent<ControllableRenderer>();
			}
			cr.parent = this;
		} else {
			Debug.LogWarning("You have a Controllable with a weird renderer (not mesh)");
		}
	}
	
	protected virtual void Start() {
		Reset();
	}
	
	protected virtual void Update() {
		if(control == state.PLAYER) {
			PlayerUpdate();
		} else {
			AIUpdate();
		}
	}

	protected virtual void FixedUpdate() {
		if(control == state.PLAYER) {
			PlayerFixedUpdate();
		} else {
			AIFixedUpdate();
		}
	}

	#region Update methods

	protected virtual void PlayerUpdate() {
		SetControls();
		sprinting = sprintKey;  // later change this to account for stamina
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
		cc.Move(velocity);  // T R I G G E R S   C O L L I S I O N   D E T E C T I O N  (AND CAN SET ONGROUND TO TRUE)

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
			//Debug.Log("onGround: " + onGround + "\nnotOnSlope: " + notOnSlope);
			//Debug.Log("Increasing gravity: " + upMov);
		} else {
			upMov = -grav;
			//if(jKey) Debug.LogWarning("Apex\njKey = " + jKey + "\nonGround = " + onGround + "\ncanStillJump = " + canStillJump);
		}
		//sprintKey = false;
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
		Vector3 tempForward = Vector3.zero;
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
		}

		// give control to this object
		control = state.PLAYER;
		cam.idistance = camDistance;
		cam.lookAt = camLook;
		currentPlayer = this;
	}

	protected virtual void Die() {
		Destroy(gameObject);
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
}
