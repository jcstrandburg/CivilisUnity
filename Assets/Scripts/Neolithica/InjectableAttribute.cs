using System;

namespace Neolithica {
    /// <summary>
    /// Indicates that a field should be injected from the GameFactory
    /// </summary>
    /// <author>Justin Strandburg</author>
    [AttributeUsage(AttributeTargets.Field|AttributeTargets.Property)]
    public class Injectable : Attribute {
    }
}
