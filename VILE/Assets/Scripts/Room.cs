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
 * the edges of the map and thus cutting parts of them off. Dependent big room
 * parts must have a frequency of 0.
 * 
 */
public class Room : MonoBehaviour {
	
	// READ ONLY (I'd use readonly but that makes stuff invisible in the inspector)
	[Range(0, 1)] public float frequency = 1f;			// how common this room is (0 for start and end room)
	public int limit = -1;								// how many of this room there can be (negative for unlimited)
	public bool indep = true;							// whether this room is the head of a big room, or a single small room
	public bool necessary = false;						// whether the room absolutely has to spawn (end, etc.)
	public List<direction> doors;						// which sides of the room can connect to a new room

	// NOTE: For big rooms, do not make loops in these lists. i.e., if room2 is in upList of room1, don't put room1
	// in downList of room2.
	[Space]
	public List<Room> leftList = new List<Room>();      // which rooms are allowed to be to the left (empty for any)
	public List<Room> rightList = new List<Room>();     // which rooms are allowed to be to the right (empty for any)
	public List<Room> upList = new List<Room>();        // which rooms are allowed to be to the up (empty for any)
	public List<Room> downList = new List<Room>();      // which rooms are allowed to be to the down (empty for any)

	[Space]
	public List<Room> leftExclude = new List<Room>();   // which rooms are not allowed to be to the left (empty for none)
	public List<Room> rightExclude = new List<Room>();  // which rooms are not allowed to be to the right (empty for none)
	public List<Room> upExclude = new List<Room>();     // which rooms are not allowed to be to the up (empty for none)
	public List<Room> downExclude = new List<Room>();   // which rooms are not allowed to be to the down (empty for none)

	// READ + WRITE
	[HideInInspector] public int distFromStart;			// how far this room is placed from the start room
	[HideInInspector] public Vector2 coords;            // map coordinates of the room

	private readonly Vector2 left = new Vector2(-1, 0), right = new Vector2(1, 0), up = new Vector2(0, -1), down = new Vector2(0, 1);

	public enum direction {
		LEFT = 2, RIGHT = 0, UP = 1, DOWN = 3
	};



	/**Whether this room needs another room connected to it after it's placed by
	 * the map generator. Doesn't take other rooms currently on the map into account yet.
	 * 
	 * BUG FIX: Fix open big-room parts not getting added to open list by appending
	 * "|| doors.Count == 1 && IsBig()"
	 */
	public bool IsOpen() {
		return doors.Count > 1 || doors.Count == 1 && IsBig();
	}

	/**Whether this room can have a specified room in the given direction.
	 * Takes inclusion/exclusion lists into account, as well as whether the
	 * given room matches up with the opening.
	 * (either both or neither rooms have an opening into each other)
	 */
	public bool CanHave(Room that, direction d) {
		switch(d) {
			case direction.LEFT:
				return (leftList.Count == 0 || leftList.Contains(that)) && !leftExclude.Contains(that) && !(doors.Contains(direction.LEFT) ^ that.doors.Contains(direction.RIGHT));
			case direction.RIGHT:
				return (rightList.Count == 0 || rightList.Contains(that)) && !rightExclude.Contains(that) && !(doors.Contains(direction.RIGHT) ^ that.doors.Contains(direction.LEFT));
			case direction.UP:
				return (upList.Count == 0 || upList.Contains(that)) && !upExclude.Contains(that) && !(doors.Contains(direction.UP) ^ that.doors.Contains(direction.DOWN));
			case direction.DOWN:
				return (downList.Count == 0 || downList.Contains(that)) && !downExclude.Contains(that) && !(doors.Contains(direction.DOWN) ^ that.doors.Contains(direction.UP));
			default:
				Debug.LogError("Room.CanMove() - invalid direction");
				return false;
		}
	}

	/**Whether this room is the head (starting point for generator) of a big room
	 */
	public bool IsBig() {
		return !indep || (leftList.Count == 1 || rightList.Count == 1 || upList.Count == 1 || downList.Count == 1);
	}

	/**Get all other parts of a big room given the head, as well as the position of
	 * each part.
	 */
	public List<RoomPart> GetParts(Vector2 pos) {
		List<RoomPart> parts = new List<RoomPart>();

		if(!indep) {
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

	/**Whether two rooms have the same amount of openings in the same places. 
	 * i.e. whether this room could safely be replaced by that room
	 */
	public bool SameDoors(Room that) {
		if(doors.Count != that.doors.Count) return false;
		foreach(direction d in doors) {
			if(!that.doors.Contains(d)) return false;
		}
		return true;
	}
}
