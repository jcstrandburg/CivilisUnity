using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tofu.Serialization.Editor {
    public class SurrogateGenerator {

        public SurrogateGenerator(Type type, bool useSimpleTypes) {
            m_useSimpleNames = useSimpleTypes;
            m_typeName = type.Name;
            m_surrogateName = m_typeName + "Surrogate";
            m_members = GetProperties(type);
            m_namespaces = GetNameSpaces(type, m_members);
        }

        public string GetOutput() {
            StringBuilder sb = new StringBuilder();

            foreach (var n in m_namespaces) {
                WriteLine(sb, "using {0};", n);
            }
            WriteLine(sb);

            WriteLine(sb, "namespace Tofu.Serialization.Surrogates {{");
            Indent();
                WriteLine(sb, "[SerializableType, SurrogateFor(typeof({0}))]", m_typeName);
                WriteLine(sb, "public class {0} {{", m_surrogateName);
                Indent();
                    WriteLine(sb);

                    WriteLine(sb, "[SerializableMember(1)] public MonoBehaviourResolver Resolver {{ get; set; }}");
                    for (var i = 0; i < m_members.Count; i++) {
                        WriteLine(sb, "[SerializableMember({0})] public {1} {2} {{ get; set; }}", i+2, m_members[i].TypeName, m_members[i].SurrogatePropertyName);
                    }
                    WriteLine(sb);

                    WriteDownCastOperator(sb);
                    WriteLine(sb);

                    WriteUpCastOperator(sb);
                Unindent();
                WriteLine(sb, "}}");
            Unindent();
            WriteLine(sb, "}}");

            return sb.ToString();
        }

        private void WriteUpCastOperator(StringBuilder sb) {
            WriteLine(sb, "public static implicit operator {0}({1} surrogate) {{", m_typeName, m_surrogateName);
            Indent();
                WriteLine(sb, "if (surrogate == null)");
                Indent();
                    WriteLine(sb, "return null;");
                Unindent();
                WriteLine(sb);

                WriteLine(sb, "{0} x = surrogate.Resolver.Resolve<{0}>();", m_typeName);
                foreach (var members in m_members) {
                    if (members.RequiresReflection) {
                        switch (members.Kind) {
                        case MemberKind.Field:
                            WriteLine(sb, "SurrogateHelper.SetFieldValue(x, typeof({0}), \"{1}\", surrogate.{2});", members.TypeName, members.SourceFieldName, members.SurrogatePropertyName);
                            break;
                        case MemberKind.Property:
                            WriteLine(sb, "SurrogateHelper.SetPropertyValue(x, typeof({0}), \"{1}\", surrogate.{2});", members.TypeName, members.SourceFieldName, members.SurrogatePropertyName);
                            break;
                        default:
                            throw new InvalidOperationException("Unhandled MemberKind value");
                        }
                    }
                    else {
                        WriteLine(sb, "x.{0} = surrogate.{1};", members.SourceFieldName, members.SurrogatePropertyName);
                    }
                }
                WriteLine(sb);

                WriteLine(sb, "return x;");
            Unindent();
            WriteLine(sb, "}}");
        }

        private void WriteDownCastOperator(StringBuilder sb) {
            WriteLine(sb, "public static implicit operator {0}({1} value) {{", m_surrogateName, m_typeName);
            Indent();
                WriteLine(sb, "if (value == null)");
                Indent();
                    WriteLine(sb, "return null;");
                Unindent();
                WriteLine(sb);

                WriteLine(sb, "return new {0} {{", m_surrogateName);
                Indent();
                    WriteLine(sb, "Resolver = MonoBehaviourResolver.Make(value),");
                foreach (var member in m_members) {
                    if (member.RequiresReflection) {
                        switch (member.Kind) {
                            case MemberKind.Field:
                                WriteLine(sb, "{0} = SurrogateHelper.GetFieldValue(x, typeof({1}), \"{2}\");", member.SurrogatePropertyName, member.TypeName, member.SourceFieldName);
                                break;
                            case MemberKind.Property:
                                WriteLine(sb, "{0} = SurrogateHelper.GetPropertyValue(x, typeof({1}), \"{2}\");", member.SurrogatePropertyName, member.TypeName, member.SourceFieldName);
                                break;
                            default:
                                throw new InvalidOperationException("Unhandled MemberKind value");
                        }
                    }
                    else {
                        WriteLine(sb, "{0} = value.{1},", member.SurrogatePropertyName, member.SourceFieldName);
                    }
                }
                Unindent();
                WriteLine(sb, "}};");
            Unindent();
            WriteLine(sb, "}}");
        }

        private string GetFriendlyPropertyName(Type type) {
            if (type.IsGenericType) {
                var sb = new StringBuilder();

                sb.Append((m_useSimpleNames ? type.Name : type.FullName).Split('`')[0]);
                sb.Append("<");
                sb.Append(string.Join(", ", type.GetGenericArguments().Select(GetFriendlyPropertyName).ToArray()));
                sb.Append(">");

                return sb.ToString();
            }
            else {
                if (type == typeof(int))
                    return "int";
                if (type == typeof(float))
                    return "float";
                if (type == typeof(bool))
                    return "bool";
                if (type == typeof(string))
                    return "string";
                if (type == typeof(double))
                    return "double";

                return (m_useSimpleNames ? type.Name : type.FullName);
            }
                
        }

        private ReadOnlyCollection<SurrogateMember> GetProperties(Type type) {
            IEnumerable<SurrogateMember> publicFields = type
                .GetFields(BindingFlags.Instance | BindingFlags.Public)
                .WhereNotHasAttribute(typeof(NonSerializedAttribute), false)
                .Where(prop => !s_ignorableBaseTypes.Contains(prop.DeclaringType))
                .Select(fieldInfo => MapFieldToSurrogateProperty(fieldInfo, false));

            IEnumerable<SurrogateMember> privateFields = type
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .WhereHasAttribute(typeof(ForceSaveAttribute), false)
                .Where(prop => !s_ignorableBaseTypes.Contains(prop.DeclaringType))
                .Select(fieldInfo => MapFieldToSurrogateProperty(fieldInfo, true));

            IEnumerable<SurrogateMember> publicProps = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .WhereNotHasAttribute(typeof(NonSerializedAttribute), false)
                .Where(prop => !s_ignorableBaseTypes.Contains(prop.DeclaringType))
                .Where(propInfo => propInfo.GetSetMethod() != null)
                .Select(propInfo => MapPropertyToSurrogateProperty(propInfo, false));

            IEnumerable<SurrogateMember> privateProps = type
                .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
                .WhereHasAttribute(typeof(ForceSaveAttribute), false)
                .Where(prop => !s_ignorableBaseTypes.Contains(prop.DeclaringType))
                .Where(propInfo => propInfo.GetSetMethod() != null)
                .Select(propInfo => MapPropertyToSurrogateProperty(propInfo, true));

            return privateFields
                .Concat(publicFields)
                .Concat(privateProps)
                .Concat(publicProps)
                .OrderBy(prop => prop.SurrogatePropertyName)
                .ToReadOnlyCollection();
        }

        private SurrogateMember MapFieldToSurrogateProperty(FieldInfo field, bool requiresReflection) {
            if (field == null)
                throw new ArgumentNullException("field");

            return new SurrogateMember {
                SourceFieldName = field.Name,
                SurrogatePropertyName = Capitalize(field.Name),
                Type = field.FieldType,
                TypeName = GetFriendlyPropertyName(field.FieldType),
                Namespace = field.FieldType.Namespace,
                RequiresReflection = field.IsPrivate,
                Kind = MemberKind.Field
            };
        }

        private SurrogateMember MapPropertyToSurrogateProperty(PropertyInfo property, bool requiresReflection) {
            if (property == null)
                throw new ArgumentNullException("property");

            return new SurrogateMember {
                SourceFieldName = property.Name,
                SurrogatePropertyName = Capitalize(property.Name),
                Type = property.PropertyType,
                TypeName = GetFriendlyPropertyName(property.PropertyType),
                Namespace = property.PropertyType.Namespace,
                RequiresReflection = !(property.CanWrite && property.CanRead),
                Kind = MemberKind.Property
            };
        }

        private ReadOnlyCollection<string> GetNameSpaces(Type type, IEnumerable<SurrogateMember> properties) {
            var minimumNamespaces = new[] { "AqlaSerializer", type.Namespace };

            if (m_useSimpleNames) {
                return properties.SelectMany(p => GetNameSpacesForType(p.Type))
                    .Concat(minimumNamespaces)
                    .Where(n => !string.IsNullOrEmpty(n))
                    .Distinct()
                    .OrderBy(n => n)
                    .ToReadOnlyCollection();
            }
            else {
                return minimumNamespaces.ToReadOnlyCollection();
            }
        }

        private static ReadOnlyCollection<string> GetNameSpacesForType(Type type) {
            var namespaces = new List<string> { type.Namespace };

            if (type.IsGenericType)
                namespaces.AddRange(type.GetGenericArguments().SelectMany(argType => GetNameSpacesForType(argType)));

            return namespaces.ToReadOnlyCollection();
        }

        private static string Capitalize(string s) {
            return s.First().ToString().ToUpper() + s.Substring(1);
        }

        private void WriteLine(StringBuilder builder, string line = null, params object[] p) {
            if (line == null) {
                builder.AppendLine();
                return;
            }

            builder.Append(m_indentString);
            builder.AppendLine(string.Format(line, p));
        }

        private void Indent() {
            SetIndentLevel(m_indentLevel + 1);
        }

        private void Unindent() {
            SetIndentLevel(m_indentLevel - 1);
        }

        private void SetIndentLevel(int indentLevel ) {
            if (indentLevel < 0)
                throw new InvalidOperationException("Indent level must be a natural number");

            m_indentLevel = indentLevel;
            m_indentString = new string(' ', indentLevel*4);
        }

        private readonly string m_typeName;
        private readonly string m_surrogateName;
        private readonly ReadOnlyCollection<SurrogateMember> m_members;
        private readonly ReadOnlyCollection<string> m_namespaces;
        private readonly bool m_useSimpleNames;
        private int m_indentLevel = 0;
        private string m_indentString = "";

        private static readonly HashSet<Type> s_ignorableBaseTypes = new HashSet<Type>(new[] { typeof(Object), typeof(Component), typeof(MonoBehaviour) });
    }
}
