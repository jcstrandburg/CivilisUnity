using System;

namespace Tofu.Serialization {
    [AttributeUsage(AttributeTargets.Class)]
    public class SavableMonobehaviourAttribute : Attribute {
        public readonly int FieldNumber;

        public SavableMonobehaviourAttribute(int fieldNumber) {
            FieldNumber = fieldNumber;
        }
    }
}