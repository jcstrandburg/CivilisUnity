using System;
using System.Collections.ObjectModel;
using AqlaSerializer.Meta;

namespace Tofu.Serialization {
    public interface ITypeModelBuilder {
        RuntimeTypeModel BuildRuntimeTypeModel();
        ReadOnlyCollection<Type> GetSavableMonobehaviours();
    }
}