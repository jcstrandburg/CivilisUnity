#if UNITY_EDITOR
using Neolithica.Orders.Simple;
using UnityEditor;
using UnityEngine;

namespace Neolithica.MonoBehaviours.Editors {
    [CustomEditor(typeof(ActorController))]
    public class ActorControllerEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            ActorController a = (ActorController)target;
            if (GUILayout.Button("Panic")) {
                GameObject go = GameObject.Find("DumpingGround");
                var order = new SimpleMoveOrder(a, go.transform.position);
                a.Panic(order);
            }
        }
    }
}
#endif
