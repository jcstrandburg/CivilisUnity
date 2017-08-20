#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Neolithica.MonoBehaviours.Editors {
    [CustomEditor(typeof(GameController))]
    public class GameControllerEditor : Editor {

        private GameController Controller => (GameController) target;

        public void OnEnable() {
            Controller.InjectAllObjects();
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            if (GUILayout.Button("TestResources")) {
                var x = Controller.GetAllAvailableResources();
                foreach (var y in x) {
                    Debug.Log($"{y.Key} {y.Value}");
                }
            }
            if (GUILayout.Button("Initialize All Objects")) {
                Controller.InjectAllObjects();
            }
        }
    }
}
#endif
