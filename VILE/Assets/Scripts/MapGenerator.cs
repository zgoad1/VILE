using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/**Randomly generate a map of rooms.
 * If provided, use specified information.
 * 
 * NOTE: Starting room MUST be tagged "StartRoom", and ending room MUST be tagged "EndRoom"
 * 
 * TODO: Replace all area intersections with 1 conductor with dead ends
 */
public class MapGenerator : MonoBehaviour {

	public Vector2 gridSize = new Vector2(20, 80);		// width and length of grid in terms of rooms
	public int roomsToExit = 80;						// how many rooms away the exit will be
	public int minRooms = 400;							// minimum amount of rooms to generate
	public string seed = "random";						// seed to use for RNG
	public static float roomSize = 100;                 // size of standard room in units
	public static MapGenerator instance;				// Singleton instance

	public GameObject[] rooms;							// list of all room prefabs
	[HideInInspector] public int[] amounts;             // how many of each room we've placed
	[HideInInspector] public List<Room> roomInstances;  // list of all room instances placed on the map

	private List<GameObject> roomsList;					// a list data structure that's otherwise exactly the same as the rooms array
	public GameObject[,] _map;							// the map to be generated
	public static GameObject[,] map {
		get {
			return instance._map;
		}
	}
	private List<Room> open;							// rooms that still need to be closed with end rooms
	private List<Room> necessary = new List<Room>();	// rooms that have to be spawned at least once
	private Dictionary<int, List<Room>> distLists;      // lists of rooms at a certain distance from the start
	private int roomsMade = 0;                          // number of rooms generated so far
	private Vector3 tileOffset;                         // how to offset the grid to put the start room at the origin
	private List<AreaIntersection> intersections = new List<AreaIntersection>();	// run PlaceConductors() on each of these at end of GenerateMap()

	// stuff to move to a subclass that's specific to this game
	private GameObject flyingPlane;
	[SerializeField] private float flyingHeight = 24;

	public static readonly Vector2[] directions = {new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0), new Vector2(0, 1)};

	enum dIndex {
		RIGHT = 0, UP = 1, LEFT = 2, DOWN = 3
	}



	private void Reset() {
		rooms = Resources.LoadAll<GameObject>("Rooms");
		roomsList = new List<GameObject>(rooms);
		amounts = new int[rooms.Length];
		roomInstances = new List<Room>();
		_map = new GameObject[(int)gridSize.y, (int)gridSize.x];
		open = new List<Room>();
		necessary = new List<Room>();
		foreach(GameObject o in rooms) {
			Room thisRoom = o.GetComponent<Room>();
			if(thisRoom.necessary) {
				necessary.Add(thisRoom);
			}
		}
		distLists = new Dictionary<int, List<Room>>();
		roomsMade = 0;
		tileOffset = new Vector3(gridSize.x / 2, 0, -gridSize.y / 2) * (-roomSize);
		flyingPlane = GameObject.Find("Flying Plane");
		flyingPlane.transform.position = new Vector3(-roomSize / 2, flyingHeight, roomSize / 2);
		flyingPlane.transform.localScale = new Vector3(gridSize.x * roomSize / 10, 1, gridSize.y * roomSize / 10);
	}

	void Start() {
		if(instance == null) instance = this;
		else {
			Debug.LogError("There are multiple MapGenerators in the scene.");
			Destroy(this);
		}
		GenerateMap(seed);
	}
	
	public void DestroyMap() {
		// destroy old map
		Room[] toDestroy = GetComponentsInChildren<Room>();
		foreach(Room r in toDestroy) {
			//Debug.Log("Destroying " + r.gameObject.name);
			if(r != null) {
				DestroyImmediate(r.gameObject);
			} else {
				Debug.LogWarning("THAT MAKES ME SAD\na child of a room object might have an extra room component");
			}
		}
	}
	
	public void GenerateMap(string seed) {
		FindObjectOfType<GameController>().FindPlayer();
		Reset();
		DestroyMap();
		SetMapSeed(seed);
		intersections.Clear();

		// place start room and GameController.player
		Vector2 startCoords = new Vector2(Mathf.Floor(gridSize.x / 2), Mathf.Floor(9 * gridSize.y / 10));
		PlaceRoom(startCoords, Array.Find(rooms, r => r.tag == "StartRoom"));
		GameController.player.transform.position = new Vector3(startCoords.x, 0, -startCoords.y) * roomSize + tileOffset;

		// while open list is not empty, connect rooms to the first open room
		// NOTE: For this not to be an infinite loop, we need to have every possible intersection in our room list.
		Room prevRoom;	// need to keep track of this in case it's removed from the open list (can't call it open[0] later)
		while(open.Count > 0) {
			prevRoom = open[0];
			Room thisPrevRoom = open[0].GetComponent<Room>();

			// try to connect a random room to the first open room's first opening until it works
			bool success = false;
			Vector2 newCoords = open[0].coords + directions[(int)GetOpenDirection(open[0])];
			// choose a random room to place out of all the rooms that could fit there
			List<GameObject> canFit = GetFittingRooms(newCoords, roomsList);
			if(canFit.Count == 0) {
				Debug.LogError("No fitting room to attach to " + open[0] + " at " + open[0].coords + " in the direction " + GetOpenDirection(open[0]) +
					"\nIncoming ArgumentOutOfRangeException.");
				// repeat for debug
				canFit = GetFittingRooms(newCoords, roomsList);
			}
			do {
				success = TryPlaceRoom(newCoords, GetRandomRoom(canFit));
			} while(!success);

			// set distFromStart to that of the previous room plus one
			Room thisRoom = _map[(int)newCoords.y, (int)newCoords.x].GetComponent<Room>();
			// if we wanted to make this actually accurate we'd add 1 to the minimum distance
			// of the existing rooms in each direction that we have a door
			thisRoom.distFromStart = prevRoom.distFromStart + 1;

			// add this room instance to the distance dictionary
			if(!distLists.ContainsKey(thisRoom.distFromStart)) {
				distLists.Add(thisRoom.distFromStart, new List<Room>());
			}
			distLists[thisRoom.distFromStart].Add(thisRoom);
		}
		Debug.Log("Generated " + roomsMade + " rooms");

		// try again if we made too few rooms
		if(roomsMade < minRooms) {
			GenerateMap(seed == "random" ? seed : seed + "_");
		} else {
			// place exit room as close to the desired position as possible, kinda
			List<GameObject> endRooms = new List<GameObject>();
			foreach(GameObject o in rooms) {
				if(o.tag == "EndRoom") endRooms.Add(o);
			}
			foreach(GameObject endRoom in endRooms) {
				int dist = roomsToExit;
				bool success = true;
				while(!TryPlaceAtDist(dist, endRoom)) {
					dist--;
					if(dist < 1) {
						// no rooms that can be replaced with this exit room
						success = false;
						break;
					}
				}
				if(success) break;
			}

			// make sure at least one of each necessary room is placed
			while(necessary.Count > 0) {
				Room r = necessary[0];
				List<Room> similar = GetSimilarRooms(roomInstances, r);
				if(similar.Count == 0) {
					Debug.LogError("Could not place necessary room: " + r.gameObject.name);
					necessary.Remove(r);
					continue;
				}
				int nextIndex = 0;
				while(!TryReplaceRoom(similar[nextIndex], r) && ++nextIndex < similar.Count);
				if(nextIndex == similar.Count) {
					Debug.LogError("Could not place necessary room: " + r.gameObject.name);
					necessary.Remove(r);
					continue;
				}
			}

			// TODO:	- Replace AIs with one conductor with dead ends
			//			- Replace AIs that are only connected to one area type with the applicable intersection,
			//			  if possible (need all types of interseciton, e.g. not necessarily tunnels)

			// Spawn Conductors for AreaIntersections
			foreach(AreaIntersection a in intersections) {
				a.PlaceConductors();
			}
		}
	}

#region RNG stuff
	// set RNG to use specified seed (if it's been changed from "random")
	void SetMapSeed(string seed) {
		UnityEngine.Random.InitState((int)(Time.realtimeSinceStartup * 1000 + Time.time * 1000));
		int seedInt = 0;
		if(seed == "random") { 
			seed = "" + Mathf.RoundToInt(UnityEngine.Random.Range(0, 10000) % 10000);
			Debug.Log("Random seed: " + seed);
		}
		for(int i = 0; i < seed.Length; i++) {
			seedInt += seed[i] * (i + 1);
		}
		UnityEngine.Random.InitState(seedInt);
	}

	/* Randomly pick a room from a list that satisfies:
	 * - frequency > 0
	 * - independent == true
	 * - haven't already placed the max amount
	 * - won't cause map generation to finish prematurely
	 */
	GameObject GetRandomRoom(List<GameObject> list) {
		GameObject choice = null;
		do {
			int randIndex = Mathf.RoundToInt(UnityEngine.Random.Range(0, list.Count));
			float rand = UnityEngine.Random.Range(0f, 1f);
			Room thisRoom = null;
			thisRoom = list[randIndex].GetComponent<Room>();
			// choose this room if: 
			// 1. probability says so (probabilities of start and end rooms are 0)
			// 2. and it's independent
			// 3. and it hasn't reached its amount limit
			// 4. and it won't cause us to stop before generating min amount of rooms
			// 5. or it's the only door combination we CAN place
			/**BUG FIX
			 * Condition 5 previously said "or it's the only *room* we CAN place"
			 * This caused infinite loops if there were multiple rooms in the list
			 * with the same door combination.
			 */
			if(rand < thisRoom.frequency 
				&& thisRoom.indep 
				&& (thisRoom.limit < 0 || amounts[randIndex] < thisRoom.limit) 
				&& !(!thisRoom.IsOpenInitially() && roomsMade < minRooms && open.Count == 1)
				|| GetSimilarRooms(list, thisRoom).Count == list.Count)
			{
				choice = list[randIndex];
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
			return true;
		}
		return false;
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
		TryReplaceRoom(RoomAt(canReplace[randIndex].coords), r.GetComponent<Room>());
		return true;
	}

	/* A room can fit at a position if:
	 * - the position is in bounds of the map
	 * - there's not a room there already
	 * - all the room's doorways connect either to other doorways or empty spots on the map
	 * - surrounding rooms' constraints allow this room here, and they all connect to it properly
	 * - if it's a big room, all its parts fit as well
	 */
	bool CanFitAt(int x, int y, Room r, bool ignoreRoomHere = false) {
		if(!IsInBounds(x, y)) return false;						// this position is out of map bounds
		if(!ignoreRoomHere && _map[y, x] != null) return false;  // there's already a room here (and it matters in this context)
		if(r.type != Room.area.INTERSECTION) {					// since intersections have variable numbers of doors, this doesn't matter
			foreach(Room.direction d in r.doors) {
				Vector2 newCoords = new Vector2(x, y);
				if(!IsInBounds(newCoords + directions[(int)d])) return false;    // there's a doorway into the edge of the map
			}
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
				if(!CanFitAt((int)locs[i].x, (int)locs[i].y, parts[i]))
					return false;
			}
		}
		return true;
	}
	bool CanFitAt(Vector2 coords, Room r, bool ignoreRoomHere = false) {
		return CanFitAt((int)coords.x, (int)coords.y, r, ignoreRoomHere);
	}

	// Set the specified map position to the given room and check if it's open after placing it.
	// To be used with individual rooms or big room parts; only places a single room at a single position.
	void PlaceRoom(int x, int y, GameObject r) {
		_map[y, x] = Instantiate(r);
		amounts[GetIndex(r)]++;
		Room thisRoom = _map[y, x].GetComponent<Room>();
		thisRoom.coords = new Vector2(x, y);
		_map[y, x].transform.position = new Vector3(thisRoom.coords.x, 0, -thisRoom.coords.y) * roomSize + tileOffset;
		_map[y, x].transform.SetParent(transform);
		if(thisRoom is AreaIntersection) {
			intersections.Add((AreaIntersection)thisRoom);
		}

		Room.direction[] doorDirecs = { Room.direction.LEFT, Room.direction.RIGHT, Room.direction.UP, Room.direction.DOWN };

		foreach(Room.direction d in doorDirecs) {
			Vector2 newCoords = thisRoom.coords + directions[(int)d];
			if(IsInBounds(newCoords)) {
				// there may be a room here
				Room adj = RoomAt(newCoords);
				if(adj != null) {
					// there is a room here

					// handle this room being area intersection
					if(thisRoom is AreaIntersection && adj.doors.Contains(Room.GetOppositeDirection(d))) {
						((AreaIntersection)thisRoom).conductors.Add(d);
						((AreaIntersection)thisRoom).connectedAreas[(int)d] = adj.type;
					}

					// handle adjacent room being area intersection
					if(adj is AreaIntersection) {
						Room.direction dOpp = Room.GetOppositeDirection(d);
						((AreaIntersection)adj).conductors.Add(dOpp);
						((AreaIntersection)adj).connectedAreas[(int)dOpp] = thisRoom.type;
					}

					// if adjacent room was closed by this, remove it from open list
					if(!IsOpenStill(adj)) {
						open.Remove(adj);
					}
				}
			}
		}

		// if possible, remove this room from the necessary list
		Room prefabRoom = r.GetComponent<Room>();
		if(prefabRoom.necessary && necessary.Contains(prefabRoom)) {
			necessary.Remove(prefabRoom);
		}

		// place the rest of this room if applicable
		if(thisRoom.IsBig() && thisRoom.indep) {
			List<RoomPart> parts = thisRoom.GetParts(new Vector2(x, y));
			foreach(RoomPart p in parts) {
				PlaceRoom((int)p.loc.x, (int)p.loc.y, p.part);
			}
		}

		if(IsOpenStill(thisRoom)) {
			open.Add(thisRoom);
		}

		// log in roomInstances
		roomInstances.Add(thisRoom);
		roomsMade++;
	}
	void PlaceRoom(Vector2 pos, GameObject r) {
		PlaceRoom((int)pos.x, (int)pos.y, r);
	}

	// return a Room if there's a Room at this position on the map; null if there isn't
	Room RoomAt(int x, int y) {
		if(IsInBounds(x, y) && _map[y, x] != null) {
			return _map[y, x].GetComponent<Room>();
		} else {
			return null;
		}
	}
	Room RoomAt(Vector2 pos) {
		return RoomAt((int)pos.x, (int)pos.y);
	}

	// whether a the specified indices are in the bounds of the map
	bool IsInBounds(int x, int y) {
		return x >= 0 && x < _map.GetLength(1) && y >= 0 && y < _map.GetLength(0);
	}
	bool IsInBounds(Vector2 pos) {
		return IsInBounds((int)pos.x, (int)pos.y);
	}

	// the direction of the first open doorway of a room
	Room.direction GetOpenDirection(Room r) {
		if(r.IsOpenInitially() || r.gameObject.tag == "StartRoom") {
			// return the first direction where there's a door in that direction with no room beyond it
			foreach(Room.direction d in r.doors) {
				Vector2 newCoords = r.coords + directions[(int)d];
				if(RoomAt(newCoords) == null && IsInBounds(newCoords)) {
					return d;
				}
			}
		}
		return (Room.direction)(-1);
	}

	// whether a room on the map with multiple doorways still has open doorways
	// (area intersections are always considered closed, but can be placed anywhere)
	bool IsOpenStill(Room r) {
		if(r.type == Room.area.INTERSECTION) {
			// area intersections are considered open when they are connected to less than 2 rooms
			//if(!(r is AreaIntersection)) {
			//	Debug.Log("You are stupid.");
			//}
			return ((AreaIntersection)r).conductors.Count < 2 && (int)GetOpenDirection(r) != -1;
		}
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
	// don't include any part of a big room
	List<Room> GetSimilarRooms(List<Room> list, Room room) {
		List<Room> ret = new List<Room>();
		foreach(Room r in list) {
			if(r.SameDoors(room) && !r.IsBig() && !r.necessary && r.type == room.type) ret.Add(r);
		}
		return ret;
	}
	List<GameObject> GetSimilarRooms(List<GameObject> list, Room room) {
		List<GameObject> ret = new List<GameObject>();
		foreach(GameObject r in list) {
			Room rm = r.GetComponent<Room>();
			if(rm.SameDoors(room) && !rm.IsBig() && !rm.necessary && rm.type == room.type) ret.Add(r);
		}
		return ret;
	}

	// replace a room instance on the map with another
	bool TryReplaceRoom(Room replacee, Room replacer) {
		if(CanFitAt(replacee.coords, replacer, true)) {
			GameObject toDestroy = replacee.gameObject;
			Vector2 pos = toDestroy.GetComponent<Room>().coords;
			roomInstances.Remove(replacee);
			PlaceRoom((int)pos.x, (int)pos.y, replacer.gameObject);
			DestroyImmediate(toDestroy);
			return true;
		}
		return false;
	}

	// get a list of all rooms that can fit at a specified location
	// exclude rooms with a frequency of 0 (start and end, bigroom parts)
	List<GameObject> GetFittingRooms(int x, int y, List<GameObject> roomList) {
		List<GameObject> fits = new List<GameObject>();
		foreach(GameObject o in roomList) {
			Room thisRoom = o.GetComponent<Room>();
			if(CanFitAt(x, y, thisRoom) && thisRoom.frequency > 0) {
				fits.Add(o);
			}
		}
		return fits;
	}
	List<GameObject> GetFittingRooms(Vector2 pos, List<GameObject> list) {
		return GetFittingRooms((int)pos.x, (int)pos.y, list);
	}

	// get a direction that points from r1 to r2
	Room.direction DirectionTo(Room r1, Room r2) {
		Vector2 d = r2.coords - r1.coords;
		if(Mathf.Abs(d.x) > Mathf.Abs(d.y)) {
			if(d.x < 0) return Room.direction.LEFT;
			return Room.direction.RIGHT;
		}
		if(d.y < 0) return Room.direction.UP;
		return Room.direction.DOWN;
	}
#endregion
}
