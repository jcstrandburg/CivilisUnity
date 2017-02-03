using UnityEngine;
using UnityEngine.UI;
using System;

public class TextBinding : MonoBehaviour {
    public DataBindingSource source;
    public string sourceTag;
    public string format = null;

    object cachedSourceValue;
    //string cachedTextValue;
    Text text;

    void Start() {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update() {
        object val = source.GetValue(sourceTag);
        if (val != null && Convert.ToString(cachedSourceValue) != Convert.ToString(val)) {
            cachedSourceValue = val;
            if (string.IsNullOrEmpty(format)) {
                text.text = Convert.ToString(val);
            } else {
                text.text = string.Format(format, val);
            }
        }
        //else if (text.text != cachedTextValue) {
        //    cachedTextValue = text.text;
        //    source.SetValue(sourceTag, text.text);
        //}
    }
}
