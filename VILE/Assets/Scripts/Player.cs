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

		ipos = transform.position;
		prevPosition = transform.position;
		camDist = cam.idistance;

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		control = state.PLAYER;
	}

	protected override void Update() {
		base.Update();

		#region Set move directions

		if(readInput) {
			bool in1 = true, in2 = true;    // used in conjunction to determine whether the player is doing any input
			if(rightKey != 0) {
				rightMov = Mathf.Lerp(rightMov, (rightKey * speed), accel);
			} else {
				in1 = false;
				rightMov = Mathf.Lerp(rightMov, 0f, decel);
			}
			if(fwdKey != 0 || sprinting) {
				fwdMov = Mathf.Lerp(fwdMov, sprinting ? runSpeed : (fwdKey * speed), accel);
			} else {
				in2 = false;
				fwdMov = Mathf.Lerp(fwdMov, 0f, decel);
			}
			anim.SetBool("input", in1 || in2);

			// change forward's y to 0 then normalize, in case the camera is pointed down or up
			Vector3 tempForward = camTransform.forward;
			tempForward.y = 0f;

			// get movement direction vector
			if(sprinting) {
				TurnIntoLightning(true);
				movDirec = Vector3.Lerp(movDirec, tempForward.normalized * fwdMov + camTransform.right.normalized * rightMov * 10, 0.1f);
			} else {
				TurnIntoLightning(false);
				movDirec = tempForward.normalized * fwdMov + camTransform.right.normalized * rightMov;
			}
			//anim.SetFloat("speed", movDirec.magnitude);
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
	}

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
