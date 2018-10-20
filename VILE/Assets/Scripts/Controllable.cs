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
	protected bool dodgeKey;
	protected bool dodging = false;
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
	private Vector2 vec1 = Vector2.zero;
	private Vector2 vec2 = Vector2.zero;
	private Animator anim;
	#endregion

	protected void Reset() {
		camTransform = FindObjectOfType<MainCamera>().transform;
		cc = GetComponent<CharacterController>();
		cam = FindObjectOfType<CameraControl>();
		forwardTarget = movDirec;
		anim = GetComponent<Animator>();
	}

	// Use this for initialization
	protected void Start() {
		Reset();

		ipos = transform.position;
		prevPosition = transform.position;
		camDist = cam.idistance;

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	// Update is called once per frame
	protected virtual void Update() {

		//controls
		rightKey = Input.GetAxisRaw("Horizontal");
		fwdKey = Input.GetAxisRaw("Vertical");
		if(Mathf.Abs(rightKey) == 1 && Mathf.Abs(fwdKey) == 1) {
			rightKey = Mathf.Sign(rightKey) * 0.707f;
			fwdKey = Mathf.Sign(fwdKey) * 0.707f;
		}
		dodgeKey = Input.GetButtonDown("Run") ? true : dodgeKey;

		#region Set move directions

		// change forward's y to 0 then normalize, in case the camera is pointed down or up
		Vector3 tempForward = camTransform.forward;
		tempForward.y = 0f;

		if(!dodging && readInput) {
			if(rightKey != 0) {
				rightMov = Mathf.Lerp(rightMov, (rightKey * speed), accel);
			} else {
				rightMov = Mathf.Lerp(rightMov, 0f, decel);
			}
			if(fwdKey != 0) {
				fwdMov = Mathf.Lerp(fwdMov, (fwdKey * speed), accel);
			} else {
				fwdMov = Mathf.Lerp(fwdMov, 0f, decel);
			}

			// get movement direction vector
			movDirec = tempForward.normalized * fwdMov + camTransform.right.normalized * rightMov;
			//anim.SetFloat("speed", movDirec.magnitude);

			if(dodgeKey && (onGround)) {
				StartCoroutine("Dodge");
				//Debug.Log("Dodging, setting CSJ to false");
			} else if(dodgeKey) {
				//Debug.Log("Dodge failed. onGround = " + onGround + "\ncanStillJump = " + canStillJump);
			}
		}
		#endregion

		#region Pause
			if(Input.GetButtonDown("Pause")) {
			if(Cursor.lockState != CursorLockMode.Locked) {
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			} else {
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}
		#endregion

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
			// (we have to do it this way because we need their y components later) (also you can't set transform.position.y)
			vec1.x = prevPosition.x;
			vec1.y = prevPosition.z;
			vec2.x = transform.position.x;
			vec2.y = transform.position.z;
			float movDist = Vector2.Distance(vec1, vec2);
			float yDist = transform.position.y - prevPosition.y;
			if(movDist < 0.05 && onGround) {    // stop if we're (presumably) running into a wall
				transform.position = prevPosition;
			}
			anim.SetFloat("speed", movDist);
			//anim.SetFloat("yVelocity", yDist);
			movDirec = (transform.position - prevPosition).normalized;
			movDirec.y = 0;
			prevPosition = transform.position;
			SetForwardTarget();
			//Debug.Log("speed: " + anim.GetFloat("speed") + "\nmovDirec: " + movDirec);
			transform.forward = Vector3.Lerp(transform.forward, forwardTarget, 0.4f);
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
			dodgeKey = false;

			if(!dodging) {
				movDirec.x = 0f;
				movDirec.z = 0f;
			}
		#endregion
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
	protected virtual IEnumerator Dodge() {
		yield return null;
	}

	protected virtual void SetForwardTarget() {
		forwardTarget = movDirec;
	}
	#endregion

	public void Pause() {
		readInput = false;
		movDirec = Vector3.zero;
		rightMov = 0;
		fwdMov = 0;
		anim.SetFloat("speed", 0);
		cam.readInput = false;
	}

	public void Unpause() {
		readInput = true;
		cam.readInput = true;
	}
}
