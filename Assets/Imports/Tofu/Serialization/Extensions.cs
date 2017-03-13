using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Tofu.Serialization {
    public static class Utility {
        public static ReadOnlyCollection<T> CreateReadOnlyCollection<T>(params T[] members) {
            return members.ToReadOnlyCollection();
        }
    }

    public static class Extensions {
        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source) {
            return source.ToList().AsReadOnly();
        }

        public static HashSet<T> ToSet<T>(this IEnumerable<T> source) {
            return new HashSet<T>(source);
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> source) where T: class {
            return source.Where(s => s != null);
        }

        public static IEnumerable<T> WhereHasAttribute<T>(this IEnumerable<T> source, Type attributeType, bool inherit) where T : MemberInfo {
            return source.Where(type => type.GetCustomAttributes(attributeType, inherit).Any());
        }

        public static IEnumerable<T> WhereNotHasAttribute<T>(this IEnumerable<T> source, Type attributeType, bool inherit) where T : MemberInfo {
            return source.Where(type => !type.GetCustomAttributes(attributeType, inherit).Any());
        }

        public static T[] GetAttributes<T>(this Type source, bool inherit) {
            return source.GetCustomAttributes(typeof(T), inherit).Cast<T>().ToArray();
        }
    }
}