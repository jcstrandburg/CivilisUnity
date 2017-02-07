#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Neolithica.MonoBehaviours.Editors {
    [CustomEditor(typeof (Warehouse))]
    class WarehouseEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            Warehouse w = (Warehouse)target;
            if (GUILayout.Button("Normalize Contents/Limits")) {
                w.FillInContentsGaps();
            }
        }
    }
}
#endif
