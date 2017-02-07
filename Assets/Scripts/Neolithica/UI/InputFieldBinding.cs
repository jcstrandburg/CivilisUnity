using System;
using UnityEngine;
using UnityEngine.UI;

namespace Neolithica.UI {
    public class InputFieldBinding : MonoBehaviour {
        public DataBindingSource source;
        public string sourceTag;
        public string format = null;

        object cachedSourceValue;
        string cachedTextValue;
        InputField field;

        void Start() {
            field = GetComponent<InputField>();
        }

        void Update() {
            object val = source.GetValue(sourceTag);
            if (val != null && Convert.ToString(cachedSourceValue) != Convert.ToString(val)) {
                cachedSourceValue = val;
                if (string.IsNullOrEmpty(format)) {
                    field.text = Convert.ToString(val);
                }
                else {
                    field.text = string.Format(format, val);
                }
            } else if (field.text != cachedTextValue) {
                cachedTextValue = field.text;
                try {
                    source.SetValue(sourceTag, field.text);
                } catch (Exception e) {
                    Debug.Log("Failed to set data binding value from InputField");
                    Debug.Log(e);
                }
            }
        }
    }
}