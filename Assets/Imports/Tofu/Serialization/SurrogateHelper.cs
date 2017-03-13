using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tofu.Serialization {
    public class SurrogateHelper {
        public static T GetFieldValue<T>(object target, Type targetType, string name) {
            return (T)GetField(targetType, name).GetValue(target);
        }

        public static T GetPropertyValue<T>(object target, Type targetType, string name) {
            return (T)GetProperty(targetType, name).GetValue(target, null);
        }

        public static void SetFieldValue<T>(object target, Type targetType, string name, T value) {
            GetField(targetType, name).SetValue(target, value);
        }

        public static void SetPropertyValue<T>(object target, Type targetType, string name, T value) {
            GetProperty(targetType, name).SetValue(target, value, null);
        }

        private static PropertyInfo GetProperty(Type type, string name) {
            string key = MakeKey(type, name);

            if (!s_propertyInfoCache.ContainsKey(key))
                s_propertyInfoCache[key] = type.GetProperty(name);

            return s_propertyInfoCache[key];
        }

        private static FieldInfo GetField(Type type, string name) {
            string key = MakeKey(type, name);

            if (!s_fieldInfoCache.ContainsKey(key))
                s_fieldInfoCache[key] = type.GetField(name);

            return s_fieldInfoCache[key];
        }

        private static string MakeKey(Type type, string fieldOrPropName) {
            return type.FullName + fieldOrPropName;
        }

        private static Dictionary<string, FieldInfo> s_fieldInfoCache = new Dictionary<string, FieldInfo>();
        private static Dictionary<string, PropertyInfo> s_propertyInfoCache = new Dictionary<string, PropertyInfo>();
    }
}