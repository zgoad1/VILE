using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * 
 */

public class AreaIntersection : Room {
	
	public List<direction> conductors = new List<direction>();
	public Conductor[] conductorObjects = new Conductor[4];

	public void PlaceConductors() {
		foreach(direction d in conductors) {
			Vector3 newLocalPos = GameController.conductorPrefab.transform.position;
			newLocalPos.x += MapGenerator.directions[(int)d].x * MapGenerator.roomSize / 2;
			newLocalPos.z += -MapGenerator.directions[(int)d].y * MapGenerator.roomSize / 2;
			Conductor c = Instantiate(GameController.conductorPrefab).GetComponent<Conductor>();
			c.direction = d;
			c.room = this;
			Vector3 newForward = new Vector3(MapGenerator.directions[(int)d].x, 0, -MapGenerator.directions[(int)d].y);
			c.transform.position = transform.position + newLocalPos;
			c.transform.up = newForward;
			c.transform.SetParent(transform);
			conductorObjects[(int)d] = c;
		}
	}
}
