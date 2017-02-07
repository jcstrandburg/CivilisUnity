#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Neolithica.MonoBehaviours.Editors {
    [CustomEditor(typeof(NeolithicObject))]
    public class NeolithicObjectEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            NeolithicObject nobject = (NeolithicObject)target;
            if (GUILayout.Button("SnapToGround")) {
                nobject.SnapToGround(true);
            }
        }
    }
}
#endif
