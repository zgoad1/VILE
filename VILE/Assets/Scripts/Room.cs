using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**Rooms are placed on a grid where each grid tile is the size of a room.
 * 
 * Each room has a list of possible rooms that can or can't be on its left,
 * right, ahead of it, or behind it.
 * 
 * Bigger rooms can be made by combining room "parts" and setting the "List"
 * variables to ONLY include the room part that attaches to it at the corresponding
 * position. This will prevent the Map Generator from putting these rooms at
 * the edges of the map and thus cutting parts of them off.
 * 
 */
public class Room : MonoBehaviour {

	[Range(0, 1)] public float frequency = 0.2f;		// how common this room is
	public int limit = -1;                              // how many of this room there can be (negative for unlimited)
	public bool head = true;                            // whether this room is the head of a big room, or a single small room
	public bool necessary = false;                      // whether the room absolutely has to spawn (end, etc.)
	public List<direction> doors;						// which sides of the room can connect to a new room
	[HideInInspector] public Vector2 coords;			// map coordinates of the room

	// NOTE: For big rooms, do not make loops in these lists. i.e., if room2 is in upList of room1, don't put room1
	// in downList of room2.
	public List<Room> leftList = new List<Room>();      // which rooms are allowed to be to the left (empty for any)
	public List<Room> rightList = new List<Room>();     // which rooms are allowed to be to the right (empty for any)
	public List<Room> upList = new List<Room>();        // which rooms are allowed to be to the up (empty for any)
	public List<Room> downList = new List<Room>();      // which rooms are allowed to be to the down (empty for any)

	public List<Room> leftExclude = new List<Room>();   // which rooms are not allowed to be to the left (empty for none)
	public List<Room> rightExclude = new List<Room>();  // which rooms are not allowed to be to the right (empty for none)
	public List<Room> upExclude = new List<Room>();     // which rooms are not allowed to be to the up (empty for none)
	public List<Room> downExclude = new List<Room>();   // which rooms are not allowed to be to the down (empty for none)

	private Vector2 left = new Vector2(-1, 0), right = new Vector2(1, 0), up = new Vector2(0, -1), down = new Vector2(0, 1);

	// Don't use rotation, instead use Unity's new inheritance system to make prefabs that are the same as others but rotated

	public enum direction {
		LEFT, RIGHT, UP, DOWN
	};

	/**Whether this room needs another room connected to it after it's placed by
	 * the map generator. Doesn't take other rooms on the map into account.
	 */
	public bool IsOpen() {
		return doors.Count > 1;
	}

	/**Whether this room can have a specified room in the given direction
	 */
	public bool CanHave(Room r, direction d) {
		switch(d) {
			case direction.LEFT:
				return (leftList.Count == 0 || leftList.Contains(r)) && !leftExclude.Contains(r);
			case direction.RIGHT:
				return (rightList.Count == 0 || rightList.Contains(r)) && !rightExclude.Contains(r);
			case direction.UP:
				return (upList.Count == 0 || upList.Contains(r)) && !upExclude.Contains(r);
			case direction.DOWN:
				return (downList.Count == 0 || downList.Contains(r)) && !downExclude.Contains(r);
			default:
				Debug.LogError("Room.CanMove() - invalid direction");
				return false;
		}
	}

	/**Whether this room is the head (starting point for generator) of a big room
	 */
	public bool IsBig() {
		return head && (leftList.Count == 1 || rightList.Count == 1 || upList.Count == 1 || downList.Count == 1);
	}

	/**Get all other parts of a big room given the head, as well as the position of
	 * each part.
	 */
	public List<RoomPart> GetParts(Vector2 pos) {
		List<RoomPart> parts = new List<RoomPart>();

		if(!head) {
			parts.Add(new RoomPart(gameObject, pos));
		}

		if(leftList.Count == 1) {
			parts.AddRange(leftList[0].GetParts(pos + left));
		}
		if(rightList.Count == 1) {
			parts.AddRange(rightList[0].GetParts(pos + right));
		}
		if(upList.Count == 1) {
			parts.AddRange(upList[0].GetParts(pos + up));
		}
		if(downList.Count == 1) {
			parts.AddRange(downList[0].GetParts(pos + down));
		}

		return parts;
	}
}
