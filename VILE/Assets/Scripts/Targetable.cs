using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Anything that can be targeted by the player.

public class Targetable : MonoBehaviour {
	[HideInInspector] public Transform camLook; // where the reticle should move to
												// (doubles as a lookAt for the camera for Controllables)
												// in general, the center of the object
	[HideInInspector] public Vector3 screenCoords = Vector3.zero;
	[HideInInspector] public bool isTarget = false;
	[HideInInspector] public float distanceFromPlayerSquared = 0;
	[HideInInspector] public float distanceFromPlayer {
		get {
			return Mathf.Sqrt(distanceFromPlayerSquared);
		}
	}
	[HideInInspector] public float distanceFromCenter = 0;
	[HideInInspector] public bool isOnScreen = false;
	[HideInInspector] public bool canTarget = true;

	public static List<Targetable> onScreen = new List<Targetable>();



	protected virtual void Reset() {
		CamLookat camLookOb = GetComponentInChildren<CamLookat>();
		if(camLookOb == null) {
			GameObject newLookat = new GameObject("CamLookat");
			camLookOb = newLookat.AddComponent<CamLookat>();
			newLookat.transform.SetParent(transform);
			newLookat.transform.localPosition = Vector3.zero;
		}
		camLook = camLookOb.transform;
	}

	protected virtual void Update() {
		distanceFromPlayerSquared = (GameController.player.camLook.position - camLook.position).sqrMagnitude;
	}

	//private void OnDrawGizmosSelected() {
	//	Gizmos.color = Color.white;
	//	Gizmos.DrawWireSphere(camLook.position, radius);
	//}

	public virtual void SetScreenCoords() {
		screenCoords = GameController.mainCamCam.WorldToScreenPoint(camLook.position);
		screenCoords.x /= Screen.width;
		screenCoords.y /= Screen.height;
	}

	public bool IsInFrontOf(Targetable t) {
		return Helper.IsInFrontOf(t.transform, transform);
	}

	public Vector2Int GetRoomCoords() {
		return new Vector2Int(
			Mathf.FloorToInt((transform.position.x + (MapGenerator.map.GetLength(1) + 1) * MapGenerator.roomSize / 2) / MapGenerator.roomSize),
			Mathf.FloorToInt((-transform.position.z + (MapGenerator.map.GetLength(0) + 1) * MapGenerator.roomSize / 2) / MapGenerator.roomSize)
		);
	}

	public Room.area GetRoomType() {
		Vector2Int coords = GetRoomCoords();
		return MapGenerator.map[coords.y, coords.x].GetComponent<Room>().type;
	}
}
