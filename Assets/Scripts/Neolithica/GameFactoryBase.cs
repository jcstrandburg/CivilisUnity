using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.DependencyInjection {
    /// <summary>
    /// Helper factory class that handles dependency injection/service location
    /// </summary>
    public abstract class GameFactoryBase {
        [Injectable]
        public GameFactoryBase Factory => this;

        /// <summary>Injectable fields by object type</summary>
        private readonly Dictionary<Type, List<FieldInfo>> fieldInfoCache = new Dictionary<Type, List<FieldInfo>>();
        /// <summary>Injectable properties by object type</summary>
        private readonly Dictionary<Type, List<PropertyInfo>> propInfoCache = new Dictionary<Type, List<PropertyInfo>>();

        private readonly Dictionary<Type, IDependencyResolver> m_dependencyResolvers;

        protected GameFactoryBase(IEnumerable<IDependencyResolver> dependencyResolvers) {
            m_dependencyResolvers = dependencyResolvers
                .Concat(new [] { new FactoryResolver(this),  })
                .ToDictionary(resolver => resolver.DependencyType);
        }

        /// <summary>
        /// Instatiates and injects a copy of the given GameObject (assumed to be a prefab)
        /// </summary>
        /// <param name="prefab">The prefab to instantiate</param>
        /// <param name="position">Optional, the position at which to instatiate the prefab</param>
        /// <returns>The instantiated object</returns>
        public GameObject Instantiate(GameObject prefab, Vector3? position = null) {
            var instance = GameObject.Instantiate(prefab);
            instance.transform.SetParent(GameController.Instance.transform);

            if (position.HasValue)
                instance.transform.position = position.Value;

            InjectGameobject(instance);
            instance.BroadcastMessage(nameof(IOnComponentWasInjected.OnComponentWasInjected), SendMessageOptions.DontRequireReceiver);

            return instance;
        }

        /// <summary>
        /// Gets all Fields for the given type that can be injected.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private List<FieldInfo> GetInjectableFields(Type t) {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

            if (!fieldInfoCache.ContainsKey(t)) {
                fieldInfoCache[t] = t
                    .GetFields(flags)
                    .Where(field => field.IsDefined(typeof(Inject), false))
                    .ToList();
            }
            return fieldInfoCache[t];
        }

        /// <summary>
        /// Gets all Properties for the given type that can be injected. The results will be cached.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private List<PropertyInfo> GetInjectableProps(Type t) {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

            if (!propInfoCache.ContainsKey(t)) {
                propInfoCache[t] = t
                    .GetProperties(flags)
                    .Where(prop => prop.IsDefined(typeof(Inject), false))
                    .ToList();
            }
            return propInfoCache[t];
        }

        /// <summary>
        /// Injects an individual object from the Injectable fields and properties of this object
        /// </summary>
        /// <param name="injectme"></param>
        /// <returns>The object passed in</returns>
        public T InjectObject<T>(T injectme) {
            if (injectme == null) {
                throw new ArgumentNullException(nameof(injectme), "Cannot inject null object");
            }
            Type compType = injectme.GetType();

            //get fields and properties to be injected
            List<FieldInfo> compFields = GetInjectableFields(compType);
            List<PropertyInfo> compProperties = GetInjectableProps(compType);

            foreach (var compField in compFields) {
                compField.SetValue(injectme, Resolve(compField.FieldType));
            }

            foreach (var compProp in compProperties) {
                compProp.SetValue(injectme, Resolve(compProp.PropertyType), null);
            }

            return injectme;
        }

        /// <summary>
        /// Injects all fields marked with the "inject" attribute in all components in the
        /// given object and all of its children. The injected value is taken from fields
        /// or properties on this object with the "injectable" attribute
        /// </summary>
        /// <param name="obj">The object to be injected</param>
        public void InjectGameobject(GameObject obj) {
            var components = obj.GetComponentsInChildren<Component>();
            foreach (var component in components) {
                InjectObject(component);
            }
        }

        /// <summary>
        /// Adds the given component type to the given game object and then injects it with dependencies
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <returns>The component added</returns>
        public T AddComponent<T>(GameObject go) where T : MonoBehaviour {
            var t = go.AddComponent<T>();
            InjectObject(t);
            return t;
        }

        private T Resolve<T>() {
            if (!m_dependencyResolvers.ContainsKey(typeof(T)))
                throw new InvalidOperationException($"Unable to resolve type {typeof(T).Name}");

            return (T)m_dependencyResolvers[typeof(T)].Resolve();
        }

        private object Resolve(Type type) {
            if (!m_dependencyResolvers.ContainsKey(type))
                throw new InvalidOperationException($"Unable to resolve type {type.Name}");

            return m_dependencyResolvers[type].Resolve();
        }

        private class FactoryResolver : IDependencyResolver {
            public FactoryResolver(GameFactoryBase factory) {
                m_factory = factory;
            }

            public object Resolve() {
                return m_factory;
            }

            public Type DependencyType => typeof(GameFactoryBase);

            private readonly GameFactoryBase m_factory;
        }
    }
}
