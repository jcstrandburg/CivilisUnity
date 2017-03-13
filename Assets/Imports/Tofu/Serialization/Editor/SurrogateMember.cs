using System;

namespace Tofu.Serialization.Editor {
    public class SurrogateMember {
        public string SourceFieldName { get; set; }
        public string SurrogatePropertyName { get; set; }
        public Type Type { get; set; }
        public string TypeName { get; set; }
        public string Namespace { get; set; }
        public bool RequiresReflection { get; set; }
        public MemberKind Kind { get; set; }
    }
}
