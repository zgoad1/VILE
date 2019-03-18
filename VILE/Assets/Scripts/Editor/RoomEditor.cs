#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AreaIntersection))]
class AreaIntersectionEditor : Editor {
	public override void OnInspectorGUI() {
		AreaIntersection ob = (AreaIntersection)target;
		if(GUILayout.Button("Print coordinates"))
			Debug.Log(ob.coords);

		DrawDefaultInspector();
	}
}
#endif