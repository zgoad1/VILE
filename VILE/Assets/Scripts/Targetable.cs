using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Doors and Controllables.

public class Targetable : MonoBehaviour {
	[HideInInspector] public Transform camLook;    // where the reticle should move to
									// (doubles as a lookAt for the camera for Controllables)
									// in general, the center of the object
	[HideInInspector] public Vector3 screenCoords = Vector3.zero;
	[HideInInspector] public bool isTarget = false;
	[HideInInspector] public float distanceFromPlayer = 0;
	[HideInInspector] public bool isOnScreen = false;
	[HideInInspector] public bool canTarget = true;

	public static List<Targetable> onScreen = new List<Targetable>();

	protected virtual void Reset() {
		CamLookat camLookOb = GetComponentInChildren<CamLookat>();
		if(camLookOb == null) {
			GameObject newLookat = new GameObject("CamLookat");
			camLookOb = newLookat.AddComponent<CamLookat>();
			newLookat.transform.SetParent(transform);
			newLookat.transform.position = Vector3.zero;
		}
		camLook = camLookOb.transform;
	}

	public virtual void SetScreenCoords() {
		screenCoords = GameController.mainCamCam.WorldToScreenPoint(camLook.position);
		screenCoords.x /= Screen.width;
		screenCoords.y /= Screen.height;
	}

	public bool IsInFrontOf(Targetable t) {
		return Helper.IsInFrontOf(t.transform, transform);
	}

	public virtual void Damage(float damage) {
		// Damageable non-Controllable Targetables can disregard the float
		// e.g. Doors should be destroyed/melted when hit by a laser
	}
}
