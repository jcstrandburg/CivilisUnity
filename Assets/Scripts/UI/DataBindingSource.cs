using UnityEngine;
using System;
using System.Collections.Generic;

public class DataBindingSource : MonoBehaviour {
    private class Binding {
        public Func<object> getter;
        public Action<object> setter;
    }

    private Dictionary<string, Binding> bindings = new Dictionary<string, Binding>();

    public void AddBinding(string sourceTag, Func<object> getter, Action<object> setter) {
        Binding b = new Binding();
        b.setter = setter;
        b.getter = getter;
        bindings[sourceTag] =  b;
    }

	public object GetValue(string sourceTag) {
        if (bindings.ContainsKey(sourceTag)) {
            var b = bindings[sourceTag];
            return b.getter();
        } else {
            Debug.Log("Getting invalid sourceTag "+sourceTag);
            return null;
        }
    }

    public void SetValue(string sourceTag, object value) {
        if (bindings.ContainsKey(sourceTag)) {
            var b = bindings[sourceTag];
            b.setter(value);
        } else {
            Debug.Log("Attemptign to set invalid sourceTag " + sourceTag);
        }
    }
}
