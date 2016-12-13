﻿using UnityEngine;
using UnityEngine.UI;
using System;

public class SliderBinding : MonoBehaviour {
    public DataBindingSource source;
    public string sourceTag;

    object cachedSourceValue;
    float cachedSliderValue;
    Slider slider;

    void Start() {
        slider = GetComponent<Slider>();
    }

	// Update is called once per frame
	void Update () {
        object val = source.GetValue(sourceTag);
        if (slider.value != cachedSliderValue) {
            cachedSliderValue = slider.value;
            source.SetValue(sourceTag, slider.value);
        } else if (val != null && Convert.ToSingle(val) != Convert.ToSingle(cachedSourceValue)) {
            cachedSourceValue = val;
            slider.value = (float)cachedSourceValue;
        }
    }
}
