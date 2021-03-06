﻿using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Neolithica.UI {

    [CustomEditor(typeof(DataBindingSource))]
    public class DataBindingSourceEditor : Editor {
        public override void OnInspectorGUI() {
            var dbs = (DataBindingSource) target;
            foreach (var label in dbs.BindingLabels)
                GUILayout.Label($"{label}: {dbs.GetValue(label)}");
        }
    }
}
#endif
