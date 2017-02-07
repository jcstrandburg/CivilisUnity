using System;

namespace Neolithica.Serialization.Attributes {
    /// <summary>
    /// Custom addition to the SerializeHelper package. Indicates that a 
    /// field should be forced to be saved into serialization data even 
    /// though it is not technically a serializable field. This is used 
    /// primarily to force classes with SerializationSurrogates to get saved
    /// </summary>
    public class ForceSerialize : Attribute {	
    }
}
