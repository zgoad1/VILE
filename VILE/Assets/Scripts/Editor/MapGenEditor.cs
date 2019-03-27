#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
class MapGenEditor : Editor {
	public override void OnInspectorGUI() {
		MapGenerator ob = (MapGenerator)target;
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Generate"))
			ob.GenerateMap(ob.seed, ob.gridSize);
		var style = new GUIStyle(GUI.skin.button);
		style.normal.textColor = Color.red;
		if(GUILayout.Button("Destroy", style))
			ob.DestroyMap();
		GUILayout.EndHorizontal();
		
		DrawDefaultInspector();
	}
}
#endif