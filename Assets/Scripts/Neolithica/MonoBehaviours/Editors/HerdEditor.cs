#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Neolithica.MonoBehaviours.Editors {
    [CustomEditor(typeof(Herd))]
    public class HerdEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            Herd herd = (Herd)target;
            if (GUILayout.Button("RandomizePath")) {
                herd.RandomizePath();
            }
        }
    }
}
#endif
