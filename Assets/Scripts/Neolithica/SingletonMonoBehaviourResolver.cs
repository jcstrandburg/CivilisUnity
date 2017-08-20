using System;
using System.Linq;
using UnityEngine;

namespace Neolithica.DependencyInjection {
    /// <summary>
    /// Resolves a MonoBehaviour dependency to a single instance of a given type.
    /// This instance will be cached until it is destroyed. If ever the cache is dry 
    /// and there are multiple instances of type T active in the scene an exception will be thrown.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonMonoBehaviourResolver<T> : IDependencyResolver
        where T : MonoBehaviour
    {
        public SingletonMonoBehaviourResolver() : this(null) {}

        public SingletonMonoBehaviourResolver(GameObject rootObject) {
            if (rootObject != null) {
                m_rootObject = rootObject;
                m_mode = Mode.Heirarchy;
            }
            else {
                m_rootObject = null;
                m_mode = Mode.Global;
            }
        }

        public object Resolve() {
            if (m_value != null) return m_value;

            T[] instances;

            switch (m_mode) {
            case Mode.Global:
                instances = GameObject.FindObjectsOfType<T>();
                break;
            case Mode.Heirarchy:
                instances = m_rootObject.GetComponentsInChildren<T>();
                break;
            default:
                throw new InvalidOperationException(string.Format("Unexpected value {0}", m_mode));
            }

            if (instances.Length > 1)
                throw new InvalidOperationException(string.Format("Multiple instances of type {0} found in scene", typeof(T).Name));

            return m_value = instances.SingleOrDefault();
        }

        public Type DependencyType => typeof(T);

        private T m_value;
        private readonly GameObject m_rootObject;
        private readonly Mode m_mode;

        private enum Mode {
            Global,
            Heirarchy,
        }
    }
}