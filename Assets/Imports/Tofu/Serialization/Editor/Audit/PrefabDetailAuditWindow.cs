using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Tofu.Serialization.Editor.Audit {
    public class PrefabDetailAuditWindow : EditorWindow {

        public const string Title = "Prefab Details";

        public void OnEnable() {
            Refresh();
        }

        public PrefabDetailAuditWindow Refresh(GameObject prefab = null, Dictionary<Type, Type> surrogateMap = null) {

            if (prefab == null && surrogateMap == null) {
                m_viewModels = Utility.CreateReadOnlyCollection<BehaviourViewModel>();
                return this;
            }

            if (prefab == null)
                throw new ArgumentNullException("prefab");

            if (surrogateMap == null)
                throw new ArgumentNullException("surrogateMap");

            var behaviours = prefab.GetComponents<MonoBehaviour>();

            m_viewModels = behaviours.Select(b => MapToViewModel(b, surrogateMap)).ToReadOnlyCollection();
            Repaint();

            return this;
        }

        public void OnGUI() {
            foreach (BehaviourViewModel viewModel in m_viewModels) {
                GUILayout.Label(viewModel.DisplayText);
            }
        }

        private BehaviourViewModel MapToViewModel(MonoBehaviour monoBehaviour, Dictionary<Type, Type> surrogateMap) {
            return new BehaviourViewModel {
                Behaviour = monoBehaviour.GetType(),
                HasSurrogate = surrogateMap.ContainsKey(monoBehaviour.GetType())
            };
        }

        private class BehaviourViewModel {
            public string DisplayText {
                get { return string.Format("{0}{1}", Behaviour.Name, HasSurrogate ? "" : " (Missing Surrogate)"); }
            }

            public Type Behaviour { get; set; }
            public bool HasSurrogate { get; set; }
        }

        private ReadOnlyCollection<BehaviourViewModel> m_viewModels;
    }
}