using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using AqlaSerializer.Meta;
using UnityEngine;

namespace Tofu.Serialization {
    public class AttributeBasedTypeModelBuilder : TypeModelBuilderBase {

        public AttributeBasedTypeModelBuilder() : this(Assembly.GetExecutingAssembly()) {}

        public AttributeBasedTypeModelBuilder(Assembly assembly) {
            m_assembly = assembly;
        }

        public override ReadOnlyCollection<Type> GetSavableMonobehaviours() {
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            return types.Where(t => t.GetCustomAttributes(typeof(SavableMonobehaviourAttribute), false).Length > 0).ToReadOnlyCollection();
        }

        public override RuntimeTypeModel BuildRuntimeTypeModel() {
            if (s_cachedModel == null)
                s_cachedModel = DoBuildRuntimeTypeModel();

            return s_cachedModel;
        }

        private RuntimeTypeModel DoBuildRuntimeTypeModel() {
            var model = GetBaseModel();
            Type[] types = m_assembly.GetTypes();

            var savableMonobehaviourHelpers = types
                .Select(t => new { Type = t, SavableMonobehaviourAttribute = (SavableMonobehaviourAttribute)t.GetCustomAttributes(typeof(SavableMonobehaviourAttribute), false).SingleOrDefault() })
                .Where(helper => helper.SavableMonobehaviourAttribute != null)
                .ToList();

            foreach (var helper in savableMonobehaviourHelpers) {
                model[typeof(MonoBehaviour)].AddSubType(helper.SavableMonobehaviourAttribute.FieldNumber, helper.Type);
            }

            var surrogateHelpers = types
                .Select(t => new { Type = t, SurrogateForAttribute = (SurrogateForAttribute)t.GetCustomAttributes(typeof(SurrogateForAttribute), false).SingleOrDefault() })
                .Where(helper => helper.SurrogateForAttribute != null)
                .ToList();

            foreach (var helper in surrogateHelpers) {
                model[helper.SurrogateForAttribute.SurrogateForType].SetSurrogate(helper.Type);
            }

            return model;
        }

        private readonly Assembly m_assembly;
        private static RuntimeTypeModel s_cachedModel;
    }
}