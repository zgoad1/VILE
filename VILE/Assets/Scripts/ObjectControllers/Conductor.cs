using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**Transports the player through area intersections.
 * The player enters and then exits through one of four sides.
 * Upon touching the Conductor, the player is disabled and a camera animation plays:
 *    - The screen does a fade transition to a tunnel (the inside of a big wire 
 *      connected to the conductor) and plays an animation in which it traverses
 *      the tunnel.
 *    - The player is prompted for input: left, right, forward, or backward.
 *    - Another animation plays in which the camera exits through the chosen tunnel.
 * When the animation is finished, the camera transitions back to the overworld and
 * the player reappears outside the conductor they exited through.
 */
 
public class Conductor : Targetable {

	[HideInInspector] public Room.direction direction;
	public Transform zoomTransform_in, zoomTransform_out;

	private void Start() {
		zoomTransform_in.SetParent(null);
		zoomTransform_in.LookAt(transform);
		zoomTransform_in.up = Vector3.up;
		zoomTransform_in.SetParent(transform);
		zoomTransform_out.SetParent(null);
		zoomTransform_out.LookAt(transform);
		zoomTransform_out.up = Vector3.up;
		zoomTransform_out.SetParent(transform);
	}
}