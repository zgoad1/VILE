using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* A collection of helper methods that would make me a bad programmer if they
 * were scattered among other files.
 */

public class Helper {
	public static bool IsInFrontOf(Transform me, Transform inFront) {
		float angle = (me.transform.localEulerAngles.y + Mathf.Atan2(me.transform.position.z - inFront.transform.position.z, me.transform.position.x - inFront.transform.position.x) / Mathf.PI * 180 + 360) % 360;
		return angle > 180 && angle < 360;  // 270 is directly in front
	}
}
