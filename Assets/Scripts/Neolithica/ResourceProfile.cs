using System;
using AqlaSerializer;
using Neolithica.MonoBehaviours;

namespace Neolithica {
    [SerializableType]
    public class ResourceProfile : ICloneable {

        public ResourceProfile(ResourceKind resourceKind, double amount) {
            ResourceKind = resourceKind;
            Amount = amount;
        }

        public object Clone() {
            return MemberwiseClone();
        }

        [SerializableMember(1)] public ResourceKind ResourceKind;
        [SerializableMember(2)] public double Amount;
    }
}