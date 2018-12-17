using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Prereqs: Doors, instance management
 * 
 * -----
 * NOTES
 * Need to update to new Unity for instance management. Will allow for making duplicate rooms that are
 * just rotated versions of the others without having to do each one individually. Need this for placing
 * transforms to aid in pathfinding through the room.
 * -----
 * 
 * Starts at the end room.
 * 
 * Uses A* to navigate. Begins by wandering aimlessly. A random dead end is picked, the path saved, and
 * it traverses the path. Upon reaching its location, it will pause for 10 seconds and then repeat
 * the process.
 *		
 * Makes noise and can be heard from a distance.
 * 
 * If a raycast reaches Tess and she's within a 90-degree field of view, it will start chasing her.
 * If Tess is in a different room and the raycast fails, it will continue to path to that room, turn
 * in a circle, then resume wandering.
 * 
 * Tess is able to sneak up from behind and attack. If it is attacked by Tess, it will enter its 
 * battle state.
 * 
 * While chasing Tess, it will use A* to path to the room where Tess is. If it is in the same room,
 * it will enter its battle state.
 * 
 * Upon finding Tess, it will shoot a laser at her, which can't affect her when she's sprinting.
 * If the laser did not reach her, it must wait 3 seconds before firing another laser. This is Tess's
 * opportunity to damage it.
 * 
 * 
 * 
 * States: WANDER, CHASE, BATTLE, STOP, RUN
 * 
 * 
 * WANDER:
 * Traverse the path found in the previous STOP state.
 * 
 * CHASE: 
 * Continually raycast to Tess each frame.
 * A* pathfind into the last room in which a raycast connected to Tess. 
 * Enter STOP if we reach that room and Tess cannot be raycast to.
 * 
 * BATTLE:
 * Shoot laser at Tess until it hits her. Pause for 3 seconds after each missed shot. Change to
 * RUN after 3 missed shots.
 * 
 * 
 * WANDER is entered:
 *		at Start
 *		at STOP end
 *		
 * CHASE is entered:
 *		from Any if raycast to Tess hits
 *		
 * BATTLE is entered:
 *		when attacked
 *		from CHASE if close to Tess
 *		
 * STOP is entered:
 *		from WANDER if we're in the destination room
 *		from CHASE if we're in the destination room and Tess cannot be found
 *		
 * RUN is entered:
 *		from BATTLE if we just successfully attacked Tess
 *		from BATTLE if we missed the laser 3 times
 *		from Any if HP is low
 *		
 *		
 * Pathfinding from room entrance to room exit:
 * Some rooms may not be completely open, so going to the middle of the room before A*-ing to the middle
 * of the next room won't always work.
 */

public class CR : Enemy {

}
