using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Tofu.Serialization.Editor.Audit {
    public static class AuditHelper {
        public static ReadOnlyCollection<Type> GetAuditableTypes() {
            return AppDomain.CurrentDomain
                .GetAssemblies().Where(assm => assm.FullName.StartsWith("Assembly-CSharp"))
                .SelectMany(assm => assm.GetTypes())
                .Distinct()
                .ToReadOnlyCollection();
        }

        public static ReadOnlyCollection<Type> GetAuditableBehaviours() {
            var monobehaviourType = typeof(MonoBehaviour);
            return GetAuditableTypes()
                .Where(type => monobehaviourType.IsAssignableFrom(type))
                .ToReadOnlyCollection();
        }

        public static ReadOnlyCollection<Type> GetSurrogates() {
            return GetAuditableTypes()
                .Where(type => type.GetCustomAttributes(typeof(SurrogateForAttribute), false).Length > 0)
                .ToReadOnlyCollection();
        }

        public static Dictionary<Type, Type> GetSurrogateMap() {
            return GetSurrogates()
                .ToDictionary(
                    surrogate => surrogate.GetAttributes<SurrogateForAttribute>(false).Single().SurrogateForType);
        }
    }
}