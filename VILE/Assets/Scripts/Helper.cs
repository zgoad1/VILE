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

	// rotate t1 towards t2 (only on Y axis)
	public static void RotateTowards(Transform t1, Transform t2, float tightness) {
		Vector2 t1v = new Vector2(t1.position.x, t1.position.z);
		Vector2 t2v = new Vector2(t2.position.x, t2.position.z);
		float angle = Vector2.Angle(t1v, t2v);
		t1.Rotate(new Vector3(0, tightness * angle, 0));
	}

	// set this layer and all child layers that matched this layer to a new layer
	// (e.g. can set FlashEye from FlyingEnemies to Enemies but keeps laser on Effects)
	public static void SetAllLayers(GameObject g, int layer) {
		int prevLayer = g.layer;
		g.layer = layer;
		foreach(Transform t in g.transform) {
			if(t.gameObject.layer == prevLayer) t.gameObject.layer = layer;
		}
	}

	public static Targetable GetRelatedTargetable(GameObject o) {
		Targetable t;
		t = o.GetComponent<Targetable>();
		if(t != null) return t;
		t = o.GetComponentInChildren<Targetable>();
		if(t != null) return t;
		Transform p = o.transform.parent;
		while(p != null) {
			t = p.GetComponent<Targetable>();
			if(t != null) return t;
			p = p.parent;
		}
		return null;
	}
}
