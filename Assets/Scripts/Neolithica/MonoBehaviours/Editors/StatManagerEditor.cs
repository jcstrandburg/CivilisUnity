#if UNITY_EDITOR
using UnityEditor;

namespace Neolithica.MonoBehaviours.Editors {
    [CustomEditor(typeof(StatManager))]
    public class StatManagerEditor : Editor {
        public override void OnInspectorGUI() {
            StatManager my = (StatManager)target;

            foreach (var stat in my.stats.Values) {
                EditorGUILayout.LabelField(stat.name);
                EditorGUILayout.FloatField((float)stat.Value);
            }
        }
    }
}
#endif
