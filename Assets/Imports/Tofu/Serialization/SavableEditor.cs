using System;
using UnityEditor;
using UnityEngine;

namespace Tofu.Serialization.Editor {
    [CustomEditor(typeof(Savable))]
    public class SavableEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            Savable savable = (Savable)target;

            if (string.IsNullOrEmpty(savable.PrefabId))
            if (GUILayout.Button("Assign Prefab Id")) {
                savable.PrefabId = Guid.NewGuid().ToString();
            }
        }
    }
}