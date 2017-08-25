using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tofu.Serialization {
    public class SurrogateHelper {
        public static TField GetFieldValue<TSource, TField>(TSource target, string name) {
            return (TField)GetField(typeof(TSource), name).GetValue(target);
        }

        public static TProperty GetPropertyValue<TSource, TProperty>(TSource target, string name) {
            return (TProperty)GetProperty(typeof(TSource), name).GetValue(target, null);
        }

        public static void SetFieldValue<TSource, TField>(TSource target, string name, TField value) {
            GetField(typeof(TSource), name).SetValue(target, value);
        }

        public static void SetPropertyValue<TSource, TProperty>(TSource target, string name, TProperty value) {
            GetProperty(typeof(TSource), name).SetValue(target, value, null);
        }

        private static PropertyInfo GetProperty(Type type, string name) {
            string key = MakeKey(type, name);

            if (!sPropertyInfoCache.ContainsKey(key))
                sPropertyInfoCache[key] = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            return sPropertyInfoCache[key];
        }

        private static FieldInfo GetField(Type type, string name) {
            string key = MakeKey(type, name);

            if (!sFieldInfoCache.ContainsKey(key))
                sFieldInfoCache[key] = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            return sFieldInfoCache[key];
        }

        private static string MakeKey(Type type, string fieldOrPropName) {
            return type.FullName + fieldOrPropName;
        }

        private static readonly Dictionary<string, FieldInfo> sFieldInfoCache = new Dictionary<string, FieldInfo>();
        private static readonly Dictionary<string, PropertyInfo> sPropertyInfoCache = new Dictionary<string, PropertyInfo>();
    }
}