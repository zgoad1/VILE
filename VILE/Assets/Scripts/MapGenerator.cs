using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/**Randomly generate a map of rooms.
 * If provided, use specified information.
 */
public class MapGenerator : MonoBehaviour {

	public Vector2 gridSize = new Vector2(128, 128);// width and length of grid by standard room size
	public int roomsToExit = 20;                    // how many rooms away the exit will be
	public string seed = "random";                  // seed to use for RNG
	public GameObject[] rooms;						// list of all room prefabs

	private GameObject[,] map;						// the map to be generated
	private List<Room> open = new List<Room>();     // rooms that still need to be closed with end rooms
	private Vector2[] directions = {new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0), new Vector2(0, 1)};

	enum dIndex {
		RIGHT = 0, UP = 1, LEFT = 2, DOWN = 3
	};

	private void Reset() {
		rooms = Resources.LoadAll<GameObject>("Rooms");
	}

	void Start () {
		GenerateMap();
	}

	[ContextMenu("Generate")]
	void GenerateMap() {
		map = new GameObject[(int)gridSize.y, (int)gridSize.x];
		SetMapSeed(seed);                                       
		// set start room to center of map
		map[Mathf.FloorToInt(gridSize.y / 2), Mathf.FloorToInt(gridSize.x / 2)] = Array.Find(rooms, r => r.name == "Start");
		// try to place rooms (ignore non-head rooms)
		
	}

	// set RNG to use specified seed (if it's been changed from "random")
	void SetMapSeed(string seed) {
		if(seed != "random") {		// if the seed has been set by the player, use it
			int seedInt = 0;
			for(int i = 0; i < seed.Length; i++) {
				seedInt += seed[i] * i;
			}
			UnityEngine.Random.InitState(seedInt);
		}							// else I think Unity uses start time as a seed?
	}

	// return true and set the room if succeeded; return false if failed
	bool SetRoom(int x, int y, GameObject r) {
		Room thisRoom = r.GetComponent<Room>();
		if(CanGoAt(x, y, thisRoom)) {
			map[y, x] = r;
			if(thisRoom.IsBig()) {
				// place connected rooms
				List<RoomPart> parts = thisRoom.GetParts(new Vector2(x, y));
				foreach(RoomPart p in parts) {
					map[(int)p.loc.y, (int)p.loc.x] = p.part;
				}
			}
			return true;
		} else {
			return false;
		}
	}

	// return a Room if there's a Room at this position on the map; null if there isn't
	Room RoomAt(int x, int y) {
		if(IsInBounds(x, y)) {
			return map[y, x].GetComponent<Room>();
		} else {
			return null;
		}
	}

	// whether a the specified indices are in the bounds of the map
	bool IsInBounds(int x, int y) {
		return x >= 0 && x < map.GetLength(1) && y >= 0 && y < map.GetLength(0);
	}

	// whether a room can fit at a specified position
	bool CanGoAt(int x, int y, Room r) {
		if(!IsInBounds(x, y)) return false;	// this position is out of map bounds
		if(map[y, x] != null) return false;	// there's already a room here
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
			List<Room> parts = new List<Room>();		// a list to store connected room parts that also need to be checked with CanGoAt
			List<Vector2> locs = new List<Vector2>();   // for the locations of those rooms
			Vector2 myLoc = new Vector2(x, y);			// grid position of this room

			if(r.leftList.Count == 1) {                         // if the left room is mandatory,
				parts.Add(r.leftList[0]);						// call this method on it next
				locs.Add(myLoc + directions[(int)dIndex.LEFT]);
			}
			if(r.rightList.Count == 1) {						// ~right
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
				if(!CanGoAt((int)locs[i].x, (int)locs[i].y, parts[i])) return false;
			}
		}
		return true;
	}
}
