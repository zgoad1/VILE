using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
class MapGenEditor : Editor {
	public override void OnInspectorGUI() {
		MapGenerator ob = ((MapGenerator)target);
		if(GUILayout.Button("Generate"))
			ob.GenerateMap(ob.seed);
		
		DrawDefaultInspector();
	}
}