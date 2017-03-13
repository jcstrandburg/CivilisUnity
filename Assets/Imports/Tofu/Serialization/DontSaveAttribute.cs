using System;

namespace Tofu.Serialization {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
    public class DontSaveAttribute : Attribute {}
}