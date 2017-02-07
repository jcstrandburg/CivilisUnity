#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Neolithica.MonoBehaviours.Editors {
    [CustomEditor(typeof(GroundController))]
    public class GroundControllerEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            GroundController groundController = (GroundController)target;
            if (GUILayout.Button("Randomize Terrain")) {
                groundController.GenerateMap();
            }
            if (GUILayout.Button("Clear Resources")) {
                groundController.ClearResources();
            }
        }
    }
}
#endif
