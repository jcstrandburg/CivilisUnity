using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Tofu.Serialization.Editor.Audit {
    public class PrefabOverviewAuditWindow : EditorWindow {

        private enum PrefabStatus {
            MissingSavable,
            MissingId,
            MissingSurrogate,
            Savable,
        };

        public const string Title = "Prefab Audit";

        public void Refresh() {
            m_surrogateMap = AuditHelper.GetSurrogateMap();

            m_prefabsByStatus = Resources.LoadAll<GameObject>("")
                .ToLookup(prefab => GetPrefabStatus(prefab, m_surrogateMap))
                .ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());

            foreach (var status in Enum.GetValues(typeof(PrefabStatus))) {
                if (!m_prefabsByStatus.ContainsKey((PrefabStatus)status))
                    m_prefabsByStatus[(PrefabStatus)status] = new List<GameObject>();
            }

            m_missingSavableCheckboxes = m_prefabsByStatus[PrefabStatus.MissingSavable]
                .ToDictionary(prefab => prefab.GetInstanceID(), prefab => false);

            Repaint();
        }

        public void OnEnable() {
            Refresh();
        }

        public void OnGUI() {
            if (m_prefabsByStatus[PrefabStatus.MissingSavable].Count > 0) {
                EditorGUILayout.LabelField("Missing Savable MonoBehaviour");

                EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button("Check All")) {
                        foreach (var key in m_missingSavableCheckboxes.Keys.ToList())
                            m_missingSavableCheckboxes[key] = true;
                    }

                    if (GUILayout.Button("Uncheck All")) {
                        foreach (var key in m_missingSavableCheckboxes.Keys.ToList())
                            m_missingSavableCheckboxes[key] = false;
                    }

                EditorGUILayout.EndHorizontal();


                scrollPos1 = EditorGUILayout.BeginScrollView(scrollPos1, GUI.skin.textArea);

                    foreach (var prefab in m_prefabsByStatus[PrefabStatus.MissingSavable]) {
                        int id = prefab.GetInstanceID();
                        m_missingSavableCheckboxes[id] = EditorGUILayout.Toggle(prefab.name, m_missingSavableCheckboxes[id]);
                    }

                EditorGUILayout.EndScrollView();

                if (GUILayout.Button("Add Savable Behaviour")) {
                    var prefabsToAdd = m_prefabsByStatus[PrefabStatus.MissingSavable]
                        .Where(prefab => m_missingSavableCheckboxes[prefab.GetInstanceID()])
                        .ToList();

                    AddSavableComponentToPrefabs(prefabsToAdd);
                    Refresh();
                }
            }

            if (m_prefabsByStatus[PrefabStatus.MissingId].Count > 0) {
                GUILayout.Label("Missing PrefabId");
                scrollPos2 = EditorGUILayout.BeginScrollView(scrollPos2, GUI.skin.textArea);

                    foreach (var prefab in m_prefabsByStatus[PrefabStatus.MissingId])
                        GUILayout.Label(prefab.name);

                EditorGUILayout.EndScrollView();
            }

            if (m_prefabsByStatus[PrefabStatus.MissingSurrogate].Count > 0) {
                GUILayout.Label("Missing Surrogate");
                scrollPos4 = EditorGUILayout.BeginScrollView(scrollPos4, GUI.skin.textArea);

                foreach (var prefab in m_prefabsByStatus[PrefabStatus.MissingSurrogate]) {
                    if (GUILayout.Button(prefab.name)) {
                        EditorWindow.GetWindow<PrefabDetailAuditWindow>().Refresh(prefab, m_surrogateMap).Show();
                    }
                }

                EditorGUILayout.EndScrollView();
            }

            if (m_prefabsByStatus[PrefabStatus.Savable].Count > 0) {
                GUILayout.Label("Savable Prefabs");
                scrollPos3 = EditorGUILayout.BeginScrollView(scrollPos3, GUI.skin.textArea);

                    foreach (var prefab in m_prefabsByStatus[PrefabStatus.Savable])
                        GUILayout.Label(prefab.name);

                EditorGUILayout.EndScrollView();
            }

            if (GUILayout.Button("Refresh")) {
                Refresh();
            }
        }

        private static void AddSavableComponentToPrefabs(ICollection<GameObject> prefabsToAdd) {
            Debug.Log(string.Join(", ", prefabsToAdd.Select(prefab => prefab.name).ToArray()));

            foreach (var prefab in prefabsToAdd)
                prefab.AddComponent<Savable>().PrefabId = System.Guid.NewGuid().ToString();
        }

        private static PrefabStatus GetPrefabStatus(GameObject prefab, Dictionary<Type, Type> surrogateMap) {
            Savable savable = prefab.GetComponent<Savable>();

            if (savable == null)
                return PrefabStatus.MissingSavable;

            if (string.IsNullOrEmpty(savable.PrefabId))
                return PrefabStatus.MissingId;

            if (prefab.GetComponents<MonoBehaviour>().Any(component => !surrogateMap.ContainsKey(component.GetType()) && !component.GetType().GetAttributes<DontSaveAttribute>(false).Any()))
                return PrefabStatus.MissingSurrogate;

            return PrefabStatus.Savable;
        }

        private Vector2 scrollPos1;
        private Vector2 scrollPos2;
        private Vector2 scrollPos3;
        private Vector2 scrollPos4;
        private Dictionary<int, bool> m_missingSavableCheckboxes;
        private Dictionary<PrefabStatus, List<GameObject>> m_prefabsByStatus;
        private Dictionary<Type, Type> m_surrogateMap;
    }
}