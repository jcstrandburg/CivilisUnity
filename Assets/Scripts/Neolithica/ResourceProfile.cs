using System;
using Neolithica.MonoBehaviours;

namespace Neolithica {
    [Serializable]
    public class ResourceProfile : ICloneable {

        public ResourceProfile(Resource.Type type, double a) {
            this.type = type;
            amount = a;
        }

        public object Clone() {
            return MemberwiseClone();
        }

        public Resource.Type type;
        public double amount;
    }
}