#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Neolithica.Serialization {
    [CustomEditor(typeof(ObjectIdentifier))]
    public class ObjectIdentifierInspector : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            ObjectIdentifier oid = (ObjectIdentifier)target;

            if (GUILayout.Button("Find Prefab")) {
                oid.FindPrefab();
            }
        }
    }
}
#endif
