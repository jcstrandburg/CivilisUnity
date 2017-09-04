using System;
using System.Collections.Generic;
using UnityEngine;

namespace Neolithica.UI {
    public class DataBindingSource : MonoBehaviour {

        public IEnumerable<string> BindingLabels => mBindings.Keys;

        public void AddBinding(string sourceTag, Func<object> getter, Action<object> setter) {
            mBindings[sourceTag] =  new Binding {
                Setter = setter,
                Getter = getter
            };
        }

        public object GetValue(string sourceTag) {
            if (!mBindings.ContainsKey(sourceTag)) {
                Debug.Log($"Getting invalid sourceTag {sourceTag}");
                return null;
            }

            return mBindings[sourceTag].Getter();
        }

        public void SetValue(string sourceTag, object value) {
            if (!mBindings.ContainsKey(sourceTag)) {
                Debug.Log($"Attempting to set invalid sourceTag {sourceTag}");
                return;
            }

            mBindings[sourceTag].Setter(value);
        }

        private class Binding {
            public Func<object> Getter;
            public Action<object> Setter;
        }

        private readonly Dictionary<string, Binding> mBindings = new Dictionary<string, Binding>();
    }
}
