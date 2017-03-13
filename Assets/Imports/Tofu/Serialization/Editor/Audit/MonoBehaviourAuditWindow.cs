using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Tofu.Serialization.Editor.Audit {
    public class MonoBehaviourAuditWindow : EditorWindow {

        public const string Title = "MonoBehaviour Audit";

        public void OnEnable() {
            Refresh();
        }

        public void Refresh() {
            ReadOnlyCollection<Type> types = AuditHelper.GetAuditableTypes();
            ReadOnlyCollection<Type> monobehaviours = AuditHelper.GetAuditableBehaviours();

            var savableMonobehaviours = monobehaviours
                .Where(type => type.GetAttributes<SavableMonobehaviourAttribute>(false).Length > 0)
                .ToList();

            var typesWithSurrogates = types
                .Select(type => type.GetAttributes<SurrogateForAttribute>(false).SingleOrDefault())
                .WhereNotNull()
                .Select(surrogateFor => surrogateFor.SurrogateForType)
                .ToList();

            m_fieldNumberConflicts = savableMonobehaviours
                .Select(GetConflictHelper)
                .GroupBy(helper => helper.FieldNumber)
                .Select(GetConflictGroup)
                .Where(group => group.Types.Count > 1)
                .OrderByDescending(group => group.Types.Count)
                .ToReadOnlyCollection();

            m_monobehavioursMissingSavable = monobehaviours.Except(savableMonobehaviours).ToReadOnlyCollection();
            m_monobehavioursMissingSurrogate = savableMonobehaviours.Except(typesWithSurrogates).ToReadOnlyCollection();
        }

        public void OnGUI() {
            if (m_monobehavioursMissingSavable.Count > 0) {
                EditorGUILayout.LabelField("Missing Savable Attribute:");
                m_missingSavableFilter = EditorGUILayout.TextField("Filter:", m_missingSavableFilter);

                scrollPos1 = EditorGUILayout.BeginScrollView(scrollPos1, GUI.skin.textArea);
                foreach (var type in m_monobehavioursMissingSavable.Where(type => type.FullName.Contains(m_missingSavableFilter)))
                    EditorGUILayout.LabelField(type.FullName);
                EditorGUILayout.EndScrollView();
            }

            if (m_monobehavioursMissingSurrogate.Count > 0) {
                GUILayout.Label("Missing Surrogate");

                scrollPos2 = EditorGUILayout.BeginScrollView(scrollPos2, GUI.skin.textArea);
                foreach (var type in m_monobehavioursMissingSurrogate)
                    GUILayout.Label(type.FullName);
                EditorGUILayout.EndScrollView();

                m_simpleNames = EditorGUILayout.Toggle("Use Simple Names", m_simpleNames);

                if (GUILayout.Button("Make Surrogates")) {
                    MakeSurrogates();
                }
            }

            if (m_fieldNumberConflicts.Count > 0) {
                GUILayout.Label("Field Number Conflicts");

                foreach (var conflict in m_fieldNumberConflicts) {
                    GUILayout.Label(conflict.FieldNumber.ToString());
                    scrollPos3 = EditorGUILayout.BeginScrollView(scrollPos3, GUI.skin.textArea);
                    foreach (var type in conflict.Types)
                        GUILayout.Label(type.FullName);
                    EditorGUILayout.EndScrollView();
                }
            }

            if (GUILayout.Button("Refresh")) {
                Refresh();
            }
        }

        private void MakeSurrogates() {
            foreach (var type in m_monobehavioursMissingSurrogate) {
                var fileName = type.Name + "Surrogate.cs";
                var result = EditorUtility.SaveFilePanel("Select destination", "Assets", fileName, "*.cs");

                if (result.Length != 0) {
                    var gen = new SurrogateGenerator(type, m_simpleNames);
                    File.WriteAllText(result, gen.GetOutput());
                }

                Debug.Log(result);
            }
        }

        private ConflictGroup GetConflictGroup(IGrouping<int, ConflictHelper> group) {
            var conflictGroup = new ConflictGroup {
                FieldNumber = group.Key,
                Types = group.Select(helper => helper.Type).ToReadOnlyCollection()
            };
            return conflictGroup;
        }

        private ConflictHelper GetConflictHelper(Type type) {
            var attribute = type.GetAttributes<SavableMonobehaviourAttribute>(false).Single();
            var helper = new ConflictHelper { Type = type, FieldNumber = attribute.FieldNumber };
            return helper;
        }

        private class ConflictGroup {
            public int FieldNumber;
            public ReadOnlyCollection<Type> Types;
        }

        private class ConflictHelper {
            public Type Type;
            public int FieldNumber;
        }

        private Vector2 scrollPos1;
        private Vector2 scrollPos2;
        private Vector2 scrollPos3;

        private ReadOnlyCollection<Type> m_monobehavioursMissingSavable;
        private ReadOnlyCollection<Type> m_monobehavioursMissingSurrogate;
        private ReadOnlyCollection<ConflictGroup> m_fieldNumberConflicts;
        private bool m_simpleNames;
        private string m_missingSavableFilter = "";
    }
}
