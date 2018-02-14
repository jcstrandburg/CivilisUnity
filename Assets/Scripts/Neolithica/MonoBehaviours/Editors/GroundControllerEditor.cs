#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Neolithica.MonoBehaviours.Editors {
    [CustomEditor(typeof(GroundController))]
    public class GroundControllerEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            GroundController groundController = (GroundController)target;

            if (GUILayout.Button("Randomize Terrain")) {
                GameController.Instance.InjectAllObjects();
                groundController.Randomize();
                groundController.GenerateMap();
            }

            if (GUILayout.Button("Erode Terrain")) {
                GameController.Instance.InjectAllObjects();
                foreach (int i in Enumerable.Range(0, 3))
                    groundController.ErodeMap();
            }


            if (GUILayout.Button("Generate Resources")) {
                GameController.Instance.InjectAllObjects();
                groundController.GenerateResourcesAndDoodads();
            }
        }
    }
}
#endif
