using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/**Randomly generate a map of rooms.
 * If provided, use specified information.
 * 
 * NOTE: Starting room MUST be tagged "StartRoom", and ending room MUST be named "End"
 */
public class MapGenerator : MonoBehaviour {

	public Vector2 gridSize = new Vector2(128, 128);	// width and length of grid by standard room size
	public int roomsToExit = 20;						// how many rooms away the exit will be
	public int minRooms = 128;							// minimum amount of rooms to generate
	public string seed = "random";						// seed to use for RNG
	public float roomSize = 100;                        // size of standard room in units

	public GameObject[] rooms;							// list of all room prefabs
	[HideInInspector] public int[] amounts;             // how many of each room we've placed
	[HideInInspector] public List<Room> roomInstances;  // list of all room instances placed on the map

	private GameObject[,] map;							// the map to be generated
	private List<Room> open;							// rooms that still need to be closed with end rooms
	private List<Room> necessary;						// rooms that have to be spawned at least once
	private Dictionary<int, List<Room>> distLists;      // lists of rooms at a certain distance from the start
	private int roomsMade = 0;                          // number of rooms generated so far
	private Vector3 tileOffset;							// how to offset the grid to put the start room at the origin

	private readonly Vector2[] directions = {new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0), new Vector2(0, 1)};

	enum dIndex {
		RIGHT = 0, UP = 1, LEFT = 2, DOWN = 3
	};



	private void Reset() {
		Debug.LogWarning("4 - up: " + (Room.direction)(4 - (int)Room.direction.UP));
		rooms = Resources.LoadAll<GameObject>("Rooms");
		amounts = new int[rooms.Length];
		foreach(GameObject o in rooms) {
			Room thisRoom = o.GetComponent<Room>();
			if(thisRoom.necessary) necessary.Add(thisRoom);
		}
		roomInstances = new List<Room>();
		map = new GameObject[(int)gridSize.y, (int)gridSize.x];
		open = new List<Room>();
		necessary = new List<Room>();
		distLists = new Dictionary<int, List<Room>>();
		roomsMade = 0;
		tileOffset = new Vector3(gridSize.x / 2, 0, -gridSize.y / 2) * (-roomSize);
	}

	void Start () {
		GenerateMap();
	}

	[ContextMenu("Generate")]
	void GenerateMap() {
		Reset();

		// destroy old map
		// destroy all existing tiles before spawning new ones
		Room[] toDestroy = GetComponentsInChildren<Room>();
		foreach(Room r in toDestroy) {
			//Debug.Log("Destroying " + r.gameObject.name);
			DestroyImmediate(r.gameObject);
		}

		SetMapSeed(seed);                                       

		// place start room at center of map
		PlaceRoom(Mathf.FloorToInt(gridSize.x / 2), Mathf.FloorToInt(gridSize.y / 2), Array.Find(rooms, r => r.tag == "StartRoom"));

		// while open list is not empty, connect rooms to the first open room
		// NOTE: For this not to be an infinite loop, we need to have every possible intersection in our room list.
		Debug.Log("Attempting to attempt to attach rooms to start...");
		int loops = 0;
		while(open.Count > 0) {
			loops++;
			if(loops > 1000000) {
				Debug.Log("cat");
			}
			Debug.Log("open.count > 0");

			// try to connect a random room to the first open room's first opening until it works
			bool success = false;
			Vector2 newCoords = open[0].coords + directions[(int)GetOpenDirection(open[0])];
			do {
				success = TryPlaceRoom(newCoords, GetRandomRoom());
			} while(!success);

			// set distFromStart to that of the previous room plus one
			Room thisRoom = map[(int)newCoords.y, (int)newCoords.x].GetComponent<Room>();
			thisRoom.distFromStart = open[0].distFromStart + 1;

			// add this room instance to the distance dictionary
			if(!distLists.ContainsKey(thisRoom.distFromStart)) {
				distLists.Add(thisRoom.distFromStart, new List<Room>());
			}
			distLists[thisRoom.distFromStart].Add(thisRoom);
		}
		Debug.Log("Generated " + roomsMade + " rooms");

		// place exit room as close to the desired position as possible
		GameObject endRoom = Array.Find(rooms, r => r.name == "End");
		int dist = roomsToExit;
		while(!TryPlaceAtDist(dist, endRoom)) {
			dist--;
			if(dist < 1) {
				Debug.LogError("MAP GENERATION FAILED COULDN'T FIND A PLACE TO PUT EXIT HELP ME MOMMY");
				break;
			}
		}

		// make sure at least one of each necessary room is placed
		foreach(Room r in necessary) {
			List<Room> similar = GetSimilarRooms(roomInstances, r);
			if(similar.Count == 0) {
				Debug.LogError("Could not place necessary room: " + r.gameObject.name);
				continue;
			}
			int randIndex = Mathf.RoundToInt(UnityEngine.Random.Range(0, similar.Count));
			ReplaceRoom(similar[randIndex], r);
		}
	}

	#region RNG stuff
	// set RNG to use specified seed (if it's been changed from "random")
	void SetMapSeed(string seed) {
		if(seed != "random") {		// if the seed has been set by the player, use it
			int seedInt = 0;
			for(int i = 0; i < seed.Length; i++) {
				seedInt += seed[i] * i;
			}
			UnityEngine.Random.InitState(seedInt);
		}							// else I think Unity uses start time as a seed?
		Debug.Log("RNG: Setting seed to " + UnityEngine.Random.state);
	}

	/**Randomly pick a room that satisfies:
	 * - frequency > 0
	 * - independent == true
	 * - haven't already placed the max amount
	 * - won't cause map generation to finish prematurely
	 */
	GameObject GetRandomRoom() {
		GameObject choice = null;
		do {
			int randIndex = Mathf.RoundToInt(UnityEngine.Random.Range(0, rooms.Length));
			float rand = UnityEngine.Random.Range(0, 1);
			Room thisRoom = rooms[randIndex].GetComponent<Room>();
			// choose this room if: 
			// probability says so (probabilities of start and end rooms are 0)
			// and it's independent
			// and it hasn't reached its amount limit
			// and it won't cause us to stop before generating min amount of rooms
			if(rand < thisRoom.frequency 
					&& thisRoom.indep 
					&& (thisRoom.limit < 0 || amounts[randIndex] < thisRoom.limit) 
					&& !(!thisRoom.IsOpen() && roomsMade < minRooms && open.Count == 1)) 
			{
				choice = rooms[randIndex];
				if(choice.tag == "StartRoom") {
					Debug.Log("cat");
				}
			}
		} while(choice == null);
		return choice;
	}
	#endregion

	#region Room placement and related checks
	// return true and place the room if it can fit; return false if not
	bool TryPlaceRoom(int x, int y, GameObject r) {
		Room thisRoom = r.GetComponent<Room>();
		if(CanFitAt(x, y, thisRoom)) {
			// place this room on the map
			PlaceRoom(x, y, r);
			// place the rest of this room if applicable
			if(thisRoom.IsBig()) {
				List<RoomPart> parts = thisRoom.GetParts(new Vector2(x, y));
				foreach(RoomPart p in parts) {
					PlaceRoom((int)p.loc.x, (int)p.loc.y, p.part);
				}
			}
			return true;
		} else {
			return false;
		}
	}
	bool TryPlaceRoom(Vector2 pos, GameObject r) {
		return TryPlaceRoom((int)pos.x, (int)pos.y, r);
	}

	// return true and replace a room at the specified distance if there are any similar rooms it can replace
	// otherwise return false
	bool TryPlaceAtDist(int dist, GameObject r) {

		// get all rooms that are "dist" rooms away from the start
		List<Room> canReplace;
		try {
			canReplace = GetSimilarRooms(distLists[dist], r.GetComponent<Room>());
		} catch {
			// this dist is not in the dictionary (no rooms here)
			return false;
		}
		if(canReplace.Count == 0) return false;

		// choose a random room from the list to replace
		int randIndex = Mathf.RoundToInt(UnityEngine.Random.Range(0, canReplace.Count));
		ReplaceRoom(RoomAt(canReplace[randIndex].coords), r.GetComponent<Room>());
		return true;
	}

	/**A room can fit at a position if:
	 * - the position is in bounds of the map
	 * - there's not a room there already
	 * - all the room's doorways connect either to other doorways or empty spots on the map
	 * - surrounding rooms' constraints allow this room here, and they all connect to it properly
	 * - if it's a big room, all its parts fit as well
	 */
	bool CanFitAt(int x, int y, Room r) {
		if(!IsInBounds(x, y)) return false;     // this position is out of map bounds
		if(map[y, x] != null) return false;     // there's already a room here
		foreach(Room.direction d in r.doors) {
			Vector2 newCoords = new Vector2(x, y);
			if(!IsInBounds(newCoords + directions[(int)d])) return false;    // there's a doorway into the edge of the map		
			Room adj = RoomAt(r.coords + directions[(int)d]);
			if(adj != null && !adj.doors.Contains((Room.direction)(4 - (int)d))) return false;  // there's a doorway into a room that doesn't connect to this doorway
		}
		#region check constraints of surrounding rooms
		Room left = RoomAt(x - 1, y);
		if(left != null) {
			if(!left.CanHave(r, Room.direction.RIGHT)) {
				return false;
			}
		}
		Room right = RoomAt(x + 1, y);
		if(right != null) {
			if(!right.CanHave(r, Room.direction.LEFT)) {
				return false;
			}
		}
		Room up = RoomAt(x, y - 1);
		if(up != null) {
			if(!up.CanHave(r, Room.direction.DOWN)) {
				return false;
			}
		}
		Room down = RoomAt(x, y + 1);
		if(down != null) {
			if(!down.CanHave(r, Room.direction.UP)) {
				return false;
			}
		}
		#endregion
		// if this room takes up more than one space, check if it can fit where we're trying to put it
		if(r.IsBig()) {
			// check each coordinate we're about to place a room, and if they all work then continue
			List<Room> parts = new List<Room>();        // a list to store connected room parts that also need to be checked with this method
			List<Vector2> locs = new List<Vector2>();   // for the locations of those rooms
			Vector2 myLoc = new Vector2(x, y);          // grid position of this room

			if(r.leftList.Count == 1) {                         // if the left room is mandatory,
				parts.Add(r.leftList[0]);                       // call this method on it next
				locs.Add(myLoc + directions[(int)dIndex.LEFT]);
			}
			if(r.rightList.Count == 1) {                        // ~right
				parts.Add(r.rightList[0]);
				locs.Add(myLoc + directions[(int)dIndex.RIGHT]);
			}
			if(r.upList.Count == 1) {
				parts.Add(r.upList[0]);
				locs.Add(myLoc + directions[(int)dIndex.UP]);
			}
			if(r.downList.Count == 1) {
				parts.Add(r.downList[0]);
				locs.Add(myLoc + directions[(int)dIndex.DOWN]);
			}

			// recursively check all the room parts, and all their parts, etc.
			for(int i = 0; i < parts.Count; i++) {
				if(!CanFitAt((int)locs[i].x, (int)locs[i].y, parts[i])) return false;
			}
		}
		return true;
	}

	// Set the specified map position to the given room and check if it's open after placing it.
	// To be used with individual rooms or big room parts; only places a single room at a single position.
	void PlaceRoom(int x, int y, GameObject r) {
		map[y, x] = Instantiate(r);
		amounts[GetIndex(r)]++;
		Room thisRoom = map[y, x].GetComponent<Room>();
		thisRoom.coords = new Vector2(x, y);
		map[y, x].transform.position = new Vector3(thisRoom.coords.x, 0, -thisRoom.coords.y) * roomSize + tileOffset;
		map[y, x].transform.SetParent(transform);
		if(IsOpen(thisRoom)) {
			Debug.Log("Adding room to open list");
			open.Add(thisRoom);
		}
		// remove any adjacent rooms from open list if they've been closed by placing this room
		foreach(Room.direction d in thisRoom.doors) {
			Room adj = RoomAt(thisRoom.coords + directions[(int)d]);
			if(adj != null && !IsOpen(adj)) {
				open.Remove(adj);
			}
		}
		// if possible, remove this room from the necessary list
		if(thisRoom.necessary && necessary.Contains(thisRoom)) necessary.Remove(thisRoom);
		// log in roomInstances
		roomInstances.Add(thisRoom);
		roomsMade++;
	}

	// return a Room if there's a Room at this position on the map; null if there isn't
	Room RoomAt(int x, int y) {
		if(IsInBounds(x, y) && map[y, x] != null) {
			return map[y, x].GetComponent<Room>();
		} else {
			return null;
		}
	}
	Room RoomAt(Vector2 pos) {
		return RoomAt((int)pos.x, (int)pos.y);
	}

	// whether a the specified indices are in the bounds of the map
	bool IsInBounds(int x, int y) {
		return x >= 0 && x < map.GetLength(1) && y >= 0 && y < map.GetLength(0);
	}
	bool IsInBounds(Vector2 pos) {
		return IsInBounds((int)pos.x, (int)pos.y);
	}

	// the direction of the first open doorway of a room
	Room.direction GetOpenDirection(Room r) {
		if(r.IsOpen() || r.gameObject.tag == "StartRoom") {
			// return the first direction where there's a door in that direction with no room beyond it
			foreach(Room.direction d in r.doors) {
				Debug.Log("Considering door " + d + " of " + r.gameObject.name);
				if(RoomAt(r.coords + directions[(int)d]) == null) {
					Debug.Log("This door is open!!!1");
					return d;
				}
			}
		}
		return (Room.direction)(-1);
	}

	// whether a room with multiple doorways still has open doorways
	bool IsOpen(Room r) {
		return GetOpenDirection(r) != (Room.direction)(-1);
	}

	// get the index of this room in the rooms array
	int GetIndex(GameObject r) {
		for(int i = 0; i < rooms.Length; i++) {
			if(rooms[i] == r) {
				return i;
			}
		}
		return -1;
	}

	// get all rooms in a list that have doors at the same positions at (can be replaced by) the specified room
	List<Room> GetSimilarRooms(List<Room> list, Room room) {
		List<Room> ret = new List<Room>();
		foreach(Room r in list) {
			if(r.SameDoors(room)) ret.Add(r);
		}
		return ret;
	}

	// replace a room instance on the map with another
	void ReplaceRoom(Room replacee, Room replacer) {
		GameObject toDestroy = replacee.gameObject;
		Vector2 pos = toDestroy.GetComponent<Room>().coords;
		PlaceRoom((int)pos.x, (int)pos.y, replacer.gameObject);
		Destroy(toDestroy);
	}
	#endregion
}
