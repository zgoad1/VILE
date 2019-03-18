using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* A collection of helper methods that would make me a bad programmer if they
 * were scattered among other files.
 * aka scripts that should be built into unity but aren't for no reason
 */

public class Helper {
	public static float AngleBetween(Transform me, Transform other) {
		return (
			me.transform.localEulerAngles.y + Mathf.Atan2 (
				me.transform.position.z - other.transform.position.z, 
				me.transform.position.x - other.transform.position.x
			) / Mathf.PI * 180 + 360
		) % 360;
	}

	public static bool IsInFrontOf(Transform me, Transform inFront) {
		float angle = AngleBetween(me, inFront);
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

	// look through both children and parents respectively to find the first Targetable component
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

	// rotate a vector about the Y (up) axis
	public static void RotateVectorY(Vector3 toRotate, out Vector3 output, float angle) {
		Vector3 vec = toRotate;
		float radAngle = angle * Mathf.PI / 180;
		vec.x = Mathf.Cos(angle * toRotate.x) - Mathf.Sin(angle * toRotate.z);
		vec.z = Mathf.Sin(angle * toRotate.x) + Mathf.Cos(angle * toRotate.z);
		output = vec;
	}

	public static Transform RecursiveFind(Transform t, string s) {
		if(t.name == s) return t;
		foreach(Transform child in t) {
			Transform found = RecursiveFind(child, s);
			if(found != null) return found;
		}
		return null;
	}
}
