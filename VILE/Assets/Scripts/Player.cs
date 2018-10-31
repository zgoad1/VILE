using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Controllable {
	private ParticleSystem lightning;
	private ParticleSystem burst;
	private ParticleSystem head;
	private SkinnedMeshRenderer mesh;
	private bool isLightning = false;
	private Transform sprintCam;
	private EpilepsyController flasher;

	protected override void Reset() {
		base.Reset();
		lightning = GetComponentsInChildren<ParticleSystem>()[0];
		burst = GetComponentsInChildren<ParticleSystem>()[1];
		head = GetComponentsInChildren<ParticleSystem>()[2];
		mesh = GetComponentInChildren<SkinnedMeshRenderer>();
		sprintCam = GameObject.Find("SprintCam").transform;
		flasher = FindObjectOfType<EpilepsyController>();
	}

	protected override void Start() {
		base.Start();
		
		prevPosition = transform.position;
		camTransform.position = transform.position;

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		SetPlayer();
	}

	protected override void PlayerUpdate() {

		base.PlayerUpdate();
		
		SetMotion();
		SetTarget();

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

		if(Input.GetKeyDown(KeyCode.LeftControl)) {
			target = FindObjectOfType<FlashEye>();
			target.SetPlayer();
		}
	}

	protected override void SetTarget() {
		Controllable t = null;
		Debug.Log("onScreen.count: " + onScreen.Count);
		foreach(Controllable c in onScreen) {
			Debug.Log("name: " + c.gameObject.name + "\nscreen coords: " + camTransform.GetComponent<Camera>().WorldToScreenPoint(c.transform.position));
		}
	}

	protected override void SetMotion() {
		/*
		base.SetMotion();
		*/
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
		/*
		base.SetVelocity();
		*/
		// change forward's y to 0 then normalize, in case the camera is pointed down or up
		Vector3 tempForward = camTransform.forward;
		tempForward.y = 0f;

		// get movement direction vector
		if(sprinting) {
			TurnIntoLightning(true);
			velocity = Vector3.Lerp(velocity, tempForward.normalized * fwdMov + camTransform.right.normalized * rightMov * 10, 0.1f);
		} else {
			TurnIntoLightning(false);
			velocity = tempForward.normalized * fwdMov + camTransform.right.normalized * rightMov;
		}
	}

	#region Setting control input variables 
	/*
	protected override void SetSprintKey() {
		sprintKey = Input.GetButtonUp("Run") ? false : Input.GetButtonDown("Run");
	}
	*/

	#endregion

	public void Pause() {
		readInput = false;
		velocity = Vector3.zero;
		rightMov = 0;
		fwdMov = 0;
		anim.SetFloat("speed", 0);
		cam.readInput = false;
	}

	public void Unpause() {
		readInput = true;
		cam.readInput = true;
	}

	#region Turning into a lightning bolt

	private void TurnIntoLightning(bool enable) {
		if(enable && !isLightning) {
			lightning.SetParticles(new ParticleSystem.Particle[0], 0);  // destroy any active particles
			mesh.enabled = false;   // make player disappear
			lightning.Play();       // start particles
			head.Play();
			cam.SetZoomTransform(sprintCam, 0.1f);
			burst.Play();
			//flasher.FlashStart(Color.red, Color.white, -1);
			isLightning = true;     // protect this part from repeated calls
		} else if(!enable && isLightning) {
			mesh.enabled = true;    // make player reappear
			lightning.Stop();       // stop particles
			head.Stop();
			cam.SetZoomTransform(null);
			burst.Play();
			//flasher.FlashStop();
			if(rightKey <= 0.1f && fwdKey <= 0.1f) {
				anim.SetTrigger("recover");
			}
			isLightning = false;    // protect this part from repeated calls
		}
	}

	#endregion

}
