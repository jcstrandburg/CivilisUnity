using System;

namespace Tofu.Serialization {
    [AttributeUsage(AttributeTargets.Class)]
    public class SurrogateForAttribute : Attribute {
        public readonly Type SurrogateForType;

        public SurrogateForAttribute(Type surrogateForType) {
            if (!surrogateForType.IsClass)
                throw new InvalidOperationException("SurrogateForType must be a class");

            SurrogateForType = surrogateForType;
        }
    }
}