using System;
using System.Collections.Generic;
using System.Linq;
using AqlaSerializer;
using Neolithica.ScriptableObjects;
using Tofu.Serialization;
using UnityEngine;

namespace Neolithica.Serialization.Surrogates {
    [SurrogateFor(typeof(ActionProfile))]
    [SerializableType]
    public class ActionProfileSurrogate {

        [SerializableMember(1)] public string Name { get; set; }

        public static implicit operator ActionProfile(ActionProfileSurrogate surrogate) {
            if (surrogate == null)
                return null;

            return Resolve(surrogate.Name);
        }

        public static implicit operator ActionProfileSurrogate(ActionProfile value) {
            if (value == null)
                return null;

            return new ActionProfileSurrogate {
                Name = value.name
            };
        }

        private static ActionProfile Resolve(string name) {
            if (name == null)
                throw new ArgumentNullException("name");

            if (s_allActionProfiles == null)
                s_allActionProfiles = Resources.LoadAll<ActionProfile>("").ToDictionary(ap => ap.name);

            if (!s_allActionProfiles.ContainsKey(name))
                throw new ArgumentException(string.Format("Unable to resolve ActionProfile {0}", name), "name");

            return s_allActionProfiles[name];
        }

        private static Dictionary<string, ActionProfile> s_allActionProfiles;
    }
}