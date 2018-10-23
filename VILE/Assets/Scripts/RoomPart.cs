using UnityEngine;

[System.Serializable]
public class RoomPart {
	public GameObject part;	// room prefab
	public Vector2 loc;		// location on grid

	public RoomPart(GameObject p, Vector2 l) {
		part = p;
		loc = l;
	}
}
