using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tofu.Serialization {
    public class LoadGameContext : ILoadGameContext {
        public LoadGameContext(IEnumerable<GameObject> prefabs) {
            List<Savable> savableComponents = prefabs.Select(p => p.GetComponent<Savable>()).ToList();

            if (savableComponents.Any(s => s == null))
                throw new ArgumentException(string.Format("One or more of prefabs has no savable component"));
                
            m_prefabs = savableComponents.ToDictionary(s => s.PrefabId, s => s.gameObject);
        }

        public virtual GameObject MakeGameObject(string prefabName) {
            GameObject gameObject = Object.Instantiate(GetPrefab(prefabName));
            return gameObject;
        }

        protected GameObject GetPrefab(string prefabName) {
            if (!m_prefabs.ContainsKey(prefabName))
                throw new InvalidOperationException(string.Format("Unable to locate prefab {0}", prefabName));

            return m_prefabs[prefabName];
        }

        private readonly Dictionary<string, GameObject> m_prefabs;
    }
}