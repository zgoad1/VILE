using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**Doors are kept track of in MapGenerator.PlaceRoom().
 * Doors are removed from the "doors" array if there IS a room in that direction.
 * Conductors must be placed in all the removed directions.
 * 
 * TODO: Handle adjacent AreaIntersections (can't place conductors on each side)
 * e.g.
 *     =========  =========
 *   ||   |     ||    |    ||
 * < ||------!<!||!>!------||>
 *   ||   |     ||    |    ||
 *     =========  =========
 * - ! means bad
 *  actually wait this can't happen can it? because intersections can be next to
 *  any area BUT intersection
 */

public class AreaIntersection : Room {
	
	public List<direction> conductors = new List<direction>();

	public void PlaceConductors() {
		foreach(direction d in conductors) {
			Vector3 newLocalPos = GameController.conductorPrefab.transform.position;
			newLocalPos.x += MapGenerator.directions[(int)d].x * MapGenerator.roomSize / 2;
			newLocalPos.z += -MapGenerator.directions[(int)d].y * MapGenerator.roomSize / 2;
			Conductor c = Instantiate(GameController.conductorPrefab).GetComponent<Conductor>();
			c.direction = d;
			Vector3 newForward = new Vector3(MapGenerator.directions[(int)d].x, 0, -MapGenerator.directions[(int)d].y);
			c.transform.position = transform.position + newLocalPos;
			c.transform.up = newForward;
			c.transform.SetParent(transform);
		}
	}
}
