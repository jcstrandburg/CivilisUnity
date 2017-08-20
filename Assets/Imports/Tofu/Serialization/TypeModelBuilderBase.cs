using System;
using System.Collections.ObjectModel;
using System.Threading;
using AqlaSerializer.Meta;
using Tofu.Serialization.Surrogates;
using UnityEngine;

namespace Tofu.Serialization {  
    public abstract class TypeModelBuilderBase : ITypeModelBuilder {
        public abstract ReadOnlyCollection<Type> GetSavableMonobehaviours();
        public abstract RuntimeTypeModel BuildRuntimeTypeModel();

        protected RuntimeTypeModel GetBaseModel() {
            return sModelCacher.Await();
        }

        public static void CacheBaseModel() {
            sModelCacher.Start();
        }

        private static ModelCacher sModelCacher = new ModelCacher();

        private class ModelCacher {
            public void Start() {
                mThread = new Thread(Run);
                mThread.Start();
            }

            public RuntimeTypeModel Await() {
                if (mThread == null)
                    Start();

                mThread.Join();
                return mTypeModel;
            }

            private void Run() {
                var model = TypeModel.Create(true, ProtoCompatibilitySettingsValue.Default);
                model.Add(typeof(GameObject), false).SetSurrogate(typeof(GameObjectSurrogate));
                model.Add(typeof(Vector3), false).SetSurrogate(typeof(Vector3Surrogate));
                model.Add(typeof(Quaternion), false).SetSurrogate(typeof(QuaternionSurrogate));
                model.Add(typeof(MonoBehaviour), false);

                mTypeModel = model;
            }

            private Thread mThread = null;
            private RuntimeTypeModel mTypeModel;
        }
    }
}
