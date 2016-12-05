using UnityEngine;
using System.Collections;
using System;

public class DayCycleController : MonoBehaviour {

    public float daytime = 0.5f;
    public float daylength = 10.0f;
    public GameObject mainLight;
    public GameObject moonLight;
    public GameObject nightLight;

    public float x;
    public float y;
    public float x2;
    public float x3;

    public float lightFalloff = 0.4f;

    public float sunIntensity;
    public float moonIntensity;
    public float minAmbient = 0.7f;
    public float maxAmbient = 1.0f;

    public float minR=0.8f, maxR=1.0f;
    public float minG=0.8f, maxG=1.0f;
    public float minB=0.8f, maxB=1.0f;

    // Handles Start event
    void Start () {
    }

    // Handles FixedUpdate event
    void FixedUpdate () {
        daytime += Time.fixedDeltaTime / daylength;
        daytime = daytime % 1.0f;

        UpdateLighting();        
    }

    // Handles Update event
    void Update() {
        if (nightLight) {
            float a = 1.0f - Mathf.Sin((float)(daytime * Math.PI));
            nightLight.GetComponent<Light>().intensity = Mathf.Pow(a, 2.0f);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int layerMask = 1 << LayerMask.NameToLayer("Terrain");
            if (Physics.Raycast(Camera.main.transform.position, ray.direction, out hit, Mathf.Infinity, layerMask)) {
                Vector3 targetPoint = new Vector3(
                    hit.point.x,
                    Mathf.Max(0.0f, hit.point.y),
                    hit.point.z
                );
                nightLight.transform.position = targetPoint + new Vector3(0, 35.0f, 0);
            }
        }
    }

    private void UpdateLighting() {
        if (!mainLight) {
            return;
        }

        x = Mathf.Pow(Mathf.Sin((float)(daytime * Math.PI)), lightFalloff);
        RenderSettings.ambientIntensity = Mathf.Lerp(minAmbient, maxAmbient, x);
        y = Mathf.Lerp(0.55f, 1.0f, x);
        RenderSettings.ambientLight = new Color(
            Mathf.Lerp(minR, maxR, x),
            Mathf.Lerp(minG, maxB, x),
            Mathf.Lerp(minG, maxB, x)
        );

        x2 = Mathf.Sin((float)((-0.25f + daytime) * 2 * Math.PI));
        var light = mainLight.GetComponent<Light>();
        light.color = new Color(1.0f, y, y);
        light.intensity = x2 * sunIntensity;
        mainLight.transform.eulerAngles = new Vector3((daytime - 0.25f) * 360.0f, 0, 0);

        x3 = Mathf.Cos((float)((daytime) * Math.PI));
        //Debug.Log(x3);
        var light2 = moonLight.GetComponent<Light>();
        light2.color = new Color(1.0f, 1.0f, 1.0f);
        light2.intensity = x3 * moonIntensity;
        moonLight.transform.eulerAngles = new Vector3((daytime + 0.25f) * 360.0f, 0, 0);
    }
}
