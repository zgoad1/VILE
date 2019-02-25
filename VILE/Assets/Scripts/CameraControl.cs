﻿using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	public Transform lookAt;
	public Transform adjTransform;      // adjusted position
	public Transform camTransform;      // camera position (lerps towards adjusted position)
	public float rad = 0.5f;            // distance from solid to stop at
	[SerializeField] private float height = 4f;
	[HideInInspector] public float lerpFac;
	public float iLerpFac = 0.2f;
	public float camSensitivity = 1f;
	public float idistance = 14;
	[HideInInspector] public float distance;

	private Vector3 dir;
	private Quaternion rot;
	private float currentX = 0;
	private float currentY = 0;
	private float sensitivityX = 4;
	private float sensitivityY = 2;
	private Vector3 lookOffset;
	private int inverted = 1;
	private int raymask;
	public bool readInput = true;
	private float screenShake;
	private Vector3 shakeVec = Vector3.zero;
	private float shakeStart;
	private Transform zoomTransform = null;
	private float zoomLerpFac = 0.1f;
	private Vector3 zOff = Vector3.zero;	// used for zooming

	// Use this for initialization
	void Start() {
		//Reset();

		dir = new Vector3(0, 0, -distance);
		lookOffset = new Vector3(0f, height, 0f);

		lerpFac = iLerpFac;
		sensitivityX *= camSensitivity;
		sensitivityY *= camSensitivity;
		// layers to raycast upon
		raymask = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("IgnoreCollision") | 1 << LayerMask.NameToLayer("Solid");
	}

	private void Update() {

		dir = new Vector3(0, 0, -distance);
		lookOffset = new Vector3(0f, height, 0f);

		if(readInput) {
			currentX += sensitivityX * Input.GetAxis("Mouse X") * 60 * Time.deltaTime;
			currentY = Mathf.Clamp(currentY - sensitivityY * inverted * Input.GetAxis("Mouse Y") * 60 * Time.deltaTime, -60f, 75f);
		}

		// TODO: Make this an actual Input axis
		if(Input.GetKeyDown(KeyCode.I)) {
			inverted = -inverted;
		}
	}

	void LateUpdate() {
		// keep the camera from going through solid colliders
		if(zoomTransform != null) {
			camTransform.position = Vector3.Lerp(camTransform.position, zoomTransform.position, zoomLerpFac * 60 * Time.smoothDeltaTime);	// smoothly move and rotate the
			camTransform.rotation = Quaternion.Slerp(camTransform.rotation, lookAt.rotation, zoomLerpFac * 60 * Time.smoothDeltaTime);		// main camera
			currentX = lookAt.rotation.eulerAngles.y;   // keep the camera behind the player when it goes back to normal tracking mode
														// (mouse movements shouldn't affect where the camera is when we come out of
														// lightning bolt mode)
			ScreenShakeUpdate();
		} else {
			RaycastHit hit;
			Vector3 rayDir = transform.position - (lookAt.position + lookOffset);
			if(Physics.SphereCast(lookAt.position + lookOffset, rad, rayDir, out hit, idistance, raymask)) { 
				Vector3 newPos = hit.point + hit.normal * rad;
				distance = Mathf.Min(idistance, Vector3.Distance(lookAt.position + lookOffset, newPos));
				//Debug.Log("Camera raycasted upon a " + hit.transform.gameObject);
				SetCam(distance);
				adjTransform.position = newPos;
			} else {
				distance = idistance;
				SetCam(distance);
			}
			camTransform.localPosition -= zOff;
			camTransform.position = Vector3.Slerp(camTransform.position, adjTransform.position, lerpFac * 60 * Time.smoothDeltaTime);
			zOff.z = Mathf.Lerp(zOff.z, 0, 0.1f);
			camTransform.localPosition += zOff;
			ScreenShakeUpdate();
			camTransform.LookAt(lookAt);
		}
	}

	private void Reset() {
		lookAt = FindObjectOfType<Controllable>().transform;
		adjTransform = GameObject.Find("Cam Adjusted").transform;
		camTransform = FindObjectOfType<MainCamera>().transform;
	}

	void SetCam(float distance) {
		if(zoomTransform == null) {	// only allow for camera movement via mouse if we're not sprinting
			dir.z = -distance;
			rot = Quaternion.Euler(currentY, currentX, 0);

			// move adjusted camera position
			adjTransform.position = (lookAt.position + lookOffset) + rot * dir;

			// move desired camera position
			Vector3 dDir = new Vector3(0, 0, -idistance);
			transform.position = (lookAt.position + lookOffset) + rot * dDir;
		}
	}

	public void ScreenShake(float intensity) {
		// prevent weak shapes interrupting strong shakes, also eliminate negative input
		if(intensity > screenShake) {
			screenShake = intensity;
			shakeStart = Time.realtimeSinceStartup;
		}
	}

	private void ScreenShakeUpdate() {
		if(screenShake > 0.01f) {
			float timeDiff = Time.realtimeSinceStartup - shakeStart + 0.1f; // this'll be 0 the first frame, so I just added 0.1 so we don't divide by 0
			shakeVec.y = -screenShake * (0.2f / timeDiff) * Mathf.Sin(2 * Mathf.PI / (timeDiff));
			shakeVec.x = -screenShake * (0.2f / timeDiff) * Mathf.Cos(2 * Mathf.PI / (0.2f * timeDiff));
			camTransform.position += shakeVec;
			//Debug.Log("ShakeVec.y: " + shakeVec.y);
			screenShake = Mathf.Lerp(screenShake, 0, 0.03f);
		}
	}

	public void Zoom(float offset) {
		zOff.z = offset;
	}

	public void SetZoomTransform(Transform t, float zoomSpeed) {
		zoomTransform = t;
		zoomLerpFac = zoomSpeed;
	}

	public void SetZoomTransform(Transform t) {
		zoomTransform = t;
		zoomLerpFac = 0.1f;
	}
}
