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

	private bool og = false;    // whether Percy can jump
	[HideInInspector]
	public bool onGround {
		get {
			return og;
		}
		set {
			og = value;
		}
	}
	[HideInInspector] public Vector3 movDirec = Vector3.zero;    // direction of movement

	protected Transform camTransform;
	protected CharacterController cc;
	protected Vector3 ipos;   // keeps track of starting position so we can return
	protected float rightKey;
	protected float fwdKey;
	protected float rightMov = 0f;
	protected float fwdMov = 0f;
	protected float upMov = 0f;
	protected bool sprintKey = false;
	protected bool sprinting = false;
	protected Quaternion playerRot = new Quaternion(0f, 0f, 0f, 0f);
	protected Vector3 hitNormal = Vector3.zero;
	protected bool notOnSlope = false;
	protected float stopSpeed = 0.075f;
	protected CameraControl cam;
	protected float camDist;
	[HideInInspector] public List<GameObject> interactables = new List<GameObject>();
	protected bool facing = false;    // whether we're facing an interactable
	protected Transform facingTransform;
	[HideInInspector] public bool readInput = true;
	protected Vector3 prevPosition;
	protected Vector3 forwardTarget;
	protected Vector2 vec1 = Vector2.zero;
	protected Vector2 vec2 = Vector2.zero;
	protected Animator anim;
	protected Rigidbody rb;
	[HideInInspector] public state control = state.AI;

	public enum state {
		AI, PLAYER
	};
	#endregion

	protected virtual void Reset() {
		camTransform = FindObjectOfType<MainCamera>().transform;
		cc = GetComponent<CharacterController>();
		cam = FindObjectOfType<CameraControl>();
		forwardTarget = movDirec;
		anim = GetComponentInChildren<Animator>();
		rb = GetComponent<Rigidbody>();
		rb.useGravity = false;
		rb.isKinematic = true;
	}

	// Use this for initialization
	protected virtual void Start() {
		Reset();
	}

	// Update is called once per frame
	protected virtual void Update() {
		if(control == state.PLAYER) {
			//controls
			rightKey = Input.GetAxisRaw("Horizontal");
			fwdKey = Input.GetAxisRaw("Vertical");
			if(Mathf.Abs(rightKey) == 1 && Mathf.Abs(fwdKey) == 1) {    // circular movement instead of square
				rightKey = Mathf.Sign(rightKey) * 0.707f;
				fwdKey = Mathf.Sign(fwdKey) * 0.707f;
			}
			//sprintKey = Input.GetButtonDown("Run") ? true : sprintKey;
			if(Input.GetButtonDown("Run")) {
				sprintKey = true;
			} else if(Input.GetButtonUp("Run")) {
				sprintKey = false;
			}
			sprinting = sprintKey;  // later change this to accout for stamina

			#region Calculate movement
			onGround = false;

			// calculate movement
			movDirec.y = upMov;
			//Character sliding of surfaces
			float slideFriction = 0.5f;
			if(!notOnSlope) {
				movDirec.x += -upMov * hitNormal.x * (1f - slideFriction);
				movDirec.z += -upMov * hitNormal.z * (1f - slideFriction);
				hitNormal = Vector3.zero;
				onGround = false;
				//Debug.Log("Sliding");
			}
			cc.Move(movDirec);  // T R I G G E R S   C O L L I S I O N   D E T E C T I O N  (AND CAN SET ONGROUND TO TRUE)

			// speed is the distance from where we were last frame to where we are now
			//Debug.Log("MovDirec: " + movDirec);
			// remove y components of positions and set movDist to the distance between
			// (we have to do it this way because we need their y components later)
			vec1.x = prevPosition.x;
			vec1.y = prevPosition.z;
			vec2.x = transform.position.x;
			vec2.y = transform.position.z;
			float movDist = Vector2.Distance(vec1, vec2);           // to check if our movement is too small to consider

			//float yDist = transform.position.y - prevPosition.y;	// for animation (falling)
			//anim.SetFloat("yVelocity", yDist);

			if(movDist < 0.05 && onGround) {                        // stop if we're (presumably) running into a wall
				transform.position = prevPosition;
			}
			anim.SetFloat("speed", movDist);
			movDirec = (transform.position - prevPosition).normalized;
			movDirec.y = 0;
			prevPosition = transform.position;
			SetForwardTarget();
			//Debug.Log("speed: " + anim.GetFloat("speed") + "\nmovDirec: " + movDirec);
			transform.forward = Vector3.Slerp(transform.forward, forwardTarget, 0.2f);
			playerRot.y = transform.rotation.y;
			playerRot.w = transform.rotation.w;
			transform.rotation = playerRot;

			// jumping & falling
			if(!onGround || !notOnSlope) {
				upMov -= grav;
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
				movDirec.x = 0f;
				movDirec.z = 0f;
			}
			#endregion
		} else {
			// default AI stuff
		}
	}

	protected virtual void OnControllerColliderHit(ControllerColliderHit hit) {
		if(hit.gameObject.tag != "Transparent") {
			hitNormal = hit.normal;
			// notOnSlope = we're on ground level enough to walk on OR we're hitting a straight-up wall
			notOnSlope = Vector3.Angle(Vector3.up, hitNormal) <= cc.slopeLimit || Vector3.Angle(Vector3.up, hitNormal) >= 89;
			if(movDirec.y <= 0 && hit.point.y < transform.position.y + .9f) {
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

	#region Movement functions

	protected virtual void SetForwardTarget() {
		forwardTarget = movDirec;
	}
	#endregion
}
