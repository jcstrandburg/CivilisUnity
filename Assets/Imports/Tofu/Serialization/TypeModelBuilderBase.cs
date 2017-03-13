using System;
using System.Collections.ObjectModel;
using AqlaSerializer.Meta;
using Tofu.Serialization.Surrogates;
using UnityEngine;

namespace Tofu.Serialization {  
    public abstract class TypeModelBuilderBase : ITypeModelBuilder {
        public abstract ReadOnlyCollection<Type> GetSavableMonobehaviours();
        public abstract RuntimeTypeModel BuildRuntimeTypeModel();

        protected RuntimeTypeModel GetBaseModel() {
            var model = TypeModel.Create(true, ProtoCompatibilitySettingsValue.Default);

            model.Add(typeof(GameObject), false).SetSurrogate(typeof(GameObjectSurrogate));
            model.Add(typeof(Vector3), false).SetSurrogate(typeof(Vector3Surrogate));
            model.Add(typeof(Quaternion), false).SetSurrogate(typeof(QuaternionSurrogate));
            model.Add(typeof(MonoBehaviour), false);

            return model;
        }
    }
}