#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Neolithica.MonoBehaviours.Editors {
    [CustomEditor(typeof(GameController))]
    public class GameControllerEditor : Editor {

        private GameController Controller {
            get { return (GameController) target; }
        }

        public void OnEnable() {
            Controller.InitializeAllObjects();
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            if (GUILayout.Button("TestResources")) {
                var x = Controller.GetAllAvailableResources();
                foreach (var y in x) {
                    Debug.Log(y.Key + " " + y.Value);
                }
            }
            if (GUILayout.Button("Initialize All Objects")) {
                Controller.InitializeAllObjects();
            }
        }
    }
}
#endif
