using System;
using UnityEditor;

namespace Tofu.Serialization.Editor.Audit {
    public static class MenuControl {
        [MenuItem("Serialization/Monobehaviour Audit")]
        public static void Start() {
            Type[] types = { typeof(MonoBehaviourAuditWindow), typeof(PrefabOverviewAuditWindow) };

            MonoBehaviourAuditWindow window1 = EditorWindow.GetWindow<MonoBehaviourAuditWindow>(MonoBehaviourAuditWindow.Title, types);
            window1.Refresh();
            window1.Show();

            PrefabOverviewAuditWindow window2 = EditorWindow.GetWindow<PrefabOverviewAuditWindow>(PrefabOverviewAuditWindow.Title, types);
            window2.Refresh();
            window2.Show();

            PrefabDetailAuditWindow window3 = EditorWindow.GetWindow<PrefabDetailAuditWindow>(PrefabDetailAuditWindow.Title, types);
            window3.Refresh();
            window3.Show();
        }
    }
}