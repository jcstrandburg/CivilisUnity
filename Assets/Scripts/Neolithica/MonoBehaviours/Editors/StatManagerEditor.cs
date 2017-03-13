#if UNITY_EDITOR
using UnityEditor;

namespace Neolithica.MonoBehaviours.Editors {
    [CustomEditor(typeof(StatManager))]
    public class StatManagerEditor : Editor {
        public override void OnInspectorGUI() {
            StatManager my = (StatManager)target;

            foreach (var stat in my.Stats()) {
                EditorGUILayout.LabelField(stat.Name);
                double startingValue = (double)stat.Value;
                double newValue = EditorGUILayout.DoubleField(startingValue);

                if (newValue != startingValue)
                    stat.SetValue((decimal)newValue);
            }
        }
    }
}
#endif
