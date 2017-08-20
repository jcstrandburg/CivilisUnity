using System;
using System.Collections.Generic;
using Neolithica.DependencyInjection;
using Neolithica.MonoBehaviours;
using NUnit.Framework;
using UnityEngine;

namespace Neolithica.Test.Editor {
    public class NeolithicTest {

        protected GameFactoryBase Factory;
        protected GameObject dummyObject;
        protected List<GameObject> tempObjects;

        [OneTimeSetUp]
        public virtual void TestFixtureSetUp() {
        }

        [OneTimeTearDown]
        public virtual void TestTextureTearDown() {
        }

        [SetUp]
        public virtual void SetUp()	{
            tempObjects = new List<GameObject>();
            Factory = new MainGameFactory(null);
            dummyObject = new GameObject {name = "DummyObject"};
        }

        [TearDown]
        public virtual void TearDown() {
            DestroyGameObject(dummyObject);
            dummyObject = null;
            foreach (var obj in tempObjects) {
                DestroyGameObject(obj);
            }
            tempObjects.Clear();
        }

        /// <summary>
        /// Makes a StatManager with a dummy persistor
        /// </summary>
        protected StatManager MakeDummyStatManager() {
            var dummyStatManager = MakePlainComponent<StatManager>();
            dummyStatManager.SetPersistor(StatManager.DummyPersistor);
            dummyStatManager.Awake();
            return dummyStatManager;
        }

        /// <summary>
        /// Creates a new Component attached to a dummy GameOject, does not do any initialization on the Component
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected T MakePlainComponent<T>() where T : Component {
            var tempGo = new GameObject();
            tempObjects.Add(tempGo);
            tempGo.name = String.Format("Temp_{0}_object", typeof(T).Name);
            var t = tempGo.AddComponent<T>();
            return t;
        }

        /// <summary>
        /// Creates a new Component attached to a dummy GameOject and has the GameFactory inject dependencies into it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected T MakeTestComponent<T>() where T : Component {
            return Factory.InjectObject(MakePlainComponent<T>());        
        }

        /// <summary>
        /// Destroys a MonoBehaviour created through MakeTestComponent/MakePlainComponent
        /// </summary>
        /// <param name="behaviour"></param>
        protected void DestroyBehaviour(MonoBehaviour behaviour) {
            var type = behaviour.GetType();
            var onDestroy = type.GetMethod("OnDestroy");
            if (onDestroy != null) {
                onDestroy.Invoke(behaviour, null);
            }
            UnityEngine.Object.DestroyImmediate(behaviour);
        }

        /// <summary>
        /// Destroys a GameObject created through MakeTestComponent/MakePlainComponent
        /// </summary>
        /// <param name="go"></param>
        protected void DestroyGameObject(GameObject go) {
            if (go != null) {
                foreach (var behaviour in go.GetComponents<MonoBehaviour>()) {
                    if (behaviour != null) {
                        DestroyBehaviour(behaviour);
                    }
                }
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
