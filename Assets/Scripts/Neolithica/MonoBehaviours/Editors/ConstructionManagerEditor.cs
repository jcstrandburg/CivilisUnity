#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Neolithica.MonoBehaviours.Editors {
    [CustomEditor(typeof(ConstructionManager))]
    public class ConstructionManagerEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            ConstructionManager cm = (ConstructionManager)target;
            if (GUILayout.Button("Ghost Good")) {
                cm.GhostGood();
            }
            if (GUILayout.Button("Ghost Bad")) {
                cm.GhostBad();
            }
            if (GUILayout.Button("Ungost")) {
                cm.UnGhost();
            }
        }
    }
}
#endif
