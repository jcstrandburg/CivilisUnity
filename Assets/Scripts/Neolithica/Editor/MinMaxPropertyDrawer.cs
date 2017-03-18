using System.Net.NetworkInformation;
using Neolithica.Utility;
using UnityEditor;
using UnityEngine;

namespace Neolithica.Editor {
    [CustomPropertyDrawer(typeof(MinMax))]
    public class MinMaxPropertyDrawer : PropertyDrawer {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(rect, label, property);
            rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive), label);

            EditorGUI.LabelField(GetRect(rect, 0f, .15f), "Min");
            EditorGUI.PropertyField(
                GetRect(rect, 0.15f, 0.325f),
                property.FindPropertyRelative("Min"),
                GUIContent.none);
            EditorGUI.LabelField(GetRect(rect, 0.5f, .15f), "Max");
            EditorGUI.PropertyField(
                GetRect(rect, 0.65f, 0.325f),
                property.FindPropertyRelative("Max"),
                GUIContent.none);

            EditorGUI.EndProperty();
        }

        private Rect GetRect(Rect rect, float start, float width) {
            return new Rect(
                rect.position.x + rect.size.x*start,
                rect.position.y,
                rect.size.x*width,
                rect.size.y);
        }

        private static GUIContent s_minLabel = new GUIContent("Min");
        private static GUIContent s_maxLabel = new GUIContent("Max");
    }
}