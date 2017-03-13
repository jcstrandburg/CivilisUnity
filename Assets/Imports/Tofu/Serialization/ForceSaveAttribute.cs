using System;

namespace Tofu.Serialization {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ForceSaveAttribute : Attribute { }
}