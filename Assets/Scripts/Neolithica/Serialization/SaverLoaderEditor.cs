#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Neolithica.Serialization {
    [CustomEditor(typeof(SaverLoader))]
    public class SaverLoaderEditor : Editor {

        private SaverLoader sl {
            get { return (SaverLoader)target; }
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            if (GUILayout.Button("QuickSave")) {
                sl.SaveGame();
            }
            if (GUILayout.Button("QuickLoad")) {
                if (!Application.isPlaying) {
                    throw new InvalidOperationException("Must be used in play mode!");
                }
                sl.LoadGame();
            }
        }
    }
}
#endif
