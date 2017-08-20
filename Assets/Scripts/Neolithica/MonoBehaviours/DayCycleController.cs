using System;
using Neolithica.Utility;
using Tofu.Serialization;
using UnityEngine;

namespace Neolithica.MonoBehaviours {
    [SavableMonobehaviour(13)]
    public class DayCycleController : MonoBehaviour {

        public float daytime = 0.5f;
        public float daylength = 10.0f;
        public float nightLightIntensity;

        public GameObject mainLight;
        public GameObject nightLight;

        public float LightFalloff = 0.4f;
        public MinMax AmbientRed;
        public MinMax AmbientGreen;
        public MinMax AmbientBlue;

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
            if (!nightLight || Camera.main == null)
                return;

            float a = Mathf.Pow(1.0f - Mathf.Sin((float) (daytime*Math.PI)), 2.0f);
            nightLight.GetComponent<Light>().intensity = a * nightLightIntensity;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            int terrainLayerMask = 1 << LayerMask.NameToLayer("Terrain");
            RaycastHit hit;
            if (!Physics.Raycast(Camera.main.transform.position, ray.direction, out hit, Mathf.Infinity, terrainLayerMask))
                return;

            const float nightLightFloatDistance = 10.0f;
            nightLight.transform.position = new Vector3(
                hit.point.x,
                Mathf.Max(0.0f, hit.point.y) + nightLightFloatDistance,
                hit.point.z);
        }

        private void UpdateLighting() {
            float lightBrightness = Mathf.Pow(Mathf.Sin((float)(daytime * Math.PI)), LightFalloff);
            mainLight.transform.eulerAngles = new Vector3((daytime - 0.25f) * 360.0f, 0, 0);
            RenderSettings.ambientLight = new Color(
                AmbientRed.Lerp(lightBrightness),
                AmbientGreen.Lerp(lightBrightness),
                AmbientBlue.Lerp(lightBrightness));
        }
    }
}
