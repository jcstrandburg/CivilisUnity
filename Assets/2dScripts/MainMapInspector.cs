using UnityEngine;
using System.Collections;
#if unity_editor
using UnityEditor;

[CustomEditor(typeof(MainMapController))]
public class MainMapInspector : Editor {

	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		if (GUILayout.Button ("Randomize")) {
			MainMapController map = (MainMapController)target;
			map.Randomize();
			map.RefreshUVs();
		}
	}
}
#endif
