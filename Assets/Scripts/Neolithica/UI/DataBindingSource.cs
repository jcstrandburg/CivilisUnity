using System;
using System.Collections.Generic;
using UnityEngine;

namespace Neolithica.UI {
    public class DataBindingSource : MonoBehaviour {

        public IEnumerable<string> BindingLabels {
            get { return mBindings.Keys; }
        }

        public void AddBinding(string sourceTag, Func<object> getter, Action<object> setter) {
            Binding b = new Binding();
            b.setter = setter;
            b.getter = getter;
            mBindings[sourceTag] =  b;
        }

        public object GetValue(string sourceTag) {
            if (mBindings.ContainsKey(sourceTag)) {
                var b = mBindings[sourceTag];
                return b.getter();
            } else {
                Debug.Log("Getting invalid sourceTag "+sourceTag);
                return null;
            }
        }

        public void SetValue(string sourceTag, object value) {
            if (mBindings.ContainsKey(sourceTag)) {
                var b = mBindings[sourceTag];
                b.setter(value);
            } else {
                Debug.Log("Attempting to set invalid sourceTag " + sourceTag);
            }
        }

        private class Binding {
            public Func<object> getter;
            public Action<object> setter;
        }

        private Dictionary<string, Binding> mBindings = new Dictionary<string, Binding>();
    }
}
