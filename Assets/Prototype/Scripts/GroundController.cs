using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(GroundController))]
public class GroundControllerEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        GroundController groundController = (GroundController)target;
        if (GUILayout.Button("RandomizeSeed")) {
            groundController.RandomizeSeed();
        }
        if (GUILayout.Button("Randomize Terrain")) {
            groundController.RandomizeTerrain();
        }
    }
}
#endif

public class GroundController : MonoBehaviour, IPointerDownHandler {
    public string seed;
    public int seedLength = 10;
    public float snowThreshold = 0.75f, stoneThreshhold = 0.5f, grassThreshold = 0.25f, waterLevel=10.0f;
    public float floatSeed = 0.0f;
    public float distExponent = 0.4f;
    public float heightMultiplier = 20.0f;
    public float heightBiasExponent = 3.0f;

    public bool cosineTerrain = true;
    public float mountainExponent = 2.0f;
    public float mountainFrequency = 0.1f;
    public float mountainHeight = 15.0f;
    public float mountainSharpness = 2.0f;
    public float mountainFalloff = 0.5f;
    public float bumpFrequency = 1.0f;
    public float bumpHeight = 1.0f;
    public float riverFrequency = 1.0f;
    public float riverHeight = 1.0f;
    public float riverExponent = 3.0f;
    public float hillFrequency = 1.0f;
    public float hillHeight = 1.0f;
    public float hillBaseOffset = 0.2f;

    public void RandomizeSeed() {
        string s = "";

        for (int i = 0; i < seedLength; i++) {
            char newChar = (char)(Random.Range(1.0f, 255.0f));
            s += newChar;
        }
        seed = s;
        floatSeed = Random.Range(0.0f, 100.0f);
    }

    //get height in range 0..1, 0..1
    protected float GetHeight(float x, float y) {
        float xOffset = floatSeed * 1.2f;
        float yOffset = floatSeed * 0.9f;
        float dist = Mathf.Min(1.0f, Mathf.Sqrt((x - 0.5f) * (x - 0.5f) + (y - 0.5f) * (y - 0.5f))); //normalized distance from center in range 0..1.414

        float hHeight = (hillBaseOffset + Mathf.PerlinNoise(xOffset + x * hillFrequency, yOffset + y * hillFrequency)) * hillHeight;
        float bHeight = Mathf.PerlinNoise(xOffset + x * bumpFrequency, yOffset + y * bumpFrequency) * bumpHeight;
        //float rHeight = riverHeight * Mathf.Pow(Mathf.Abs(Mathf.PerlinNoise(xOffset + x * riverFrequency, yOffset + y * riverFrequency) - 0.5f), riverExponent);

        float rHeight = riverHeight * Mathf.Pow(2*Mathf.Abs(Mathf.PerlinNoise(xOffset + x * riverFrequency, yOffset + y * riverFrequency) - 0.5f), riverExponent);
        float mHeight = mountainHeight * Mathf.Pow(Mathf.PerlinNoise(xOffset + x * mountainFrequency, yOffset + y * mountainFrequency), mountainSharpness) * Mathf.Pow(dist, mountainFalloff);

        //return mHeight;
        return Mathf.Min(mHeight+hHeight, rHeight)+bHeight;
    }

    public void RandomizeTerrain() {
        Terrain terrain = GetComponent<Terrain>();
        float[,] heights = new float[terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight];

        for (int i = 0; i < terrain.terrainData.heightmapWidth; i++) { 
            for (int k = 0; k < terrain.terrainData.heightmapHeight; k++) {
                float x = (float)i / (float)terrain.terrainData.heightmapWidth;
                float y = (float)k / (float)terrain.terrainData.heightmapHeight;

                heights[i, k] = GetHeight(x, y);
            }
        }

        terrain.terrainData.SetHeights(0, 0, heights);
        GenerateSplatMap();
    }

    public float interp(float a, float b, float c) {
        float t = (c - a) / (b - a);//linear interpolation

        if (cosineTerrain) {
            t = 0.5f - Mathf.Cos(Mathf.PI * t) / 2.0f;//convert to cosine interpolation
        }

        return t;
    }

    public void GenerateSplatMap() {
        Terrain terrain = GetComponent<Terrain>();
        TerrainData terrainData = terrain.terrainData;

        // Splatmap data is stored internally as a 3d array of floats, so declare a new empty array ready for your custom splatmap data:
        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        for (int y = 0; y < terrainData.alphamapHeight; y++) {
            for (int x = 0; x < terrainData.alphamapWidth; x++) {
                // Normalise x/y coordinates to range 0-1 
                float y_01 = (float)y / (float)terrainData.alphamapHeight;
                float x_01 = (float)x / (float)terrainData.alphamapWidth;

                // Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)
                float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapHeight), Mathf.RoundToInt(x_01 * terrainData.heightmapWidth));

                // Setup an array to record the mix of texture weights at this point
                float[] splatWeights = new float[terrainData.alphamapLayers];

                //base settigns
                splatWeights[0] = 0.0f;
                splatWeights[1] = 1.0f;
                splatWeights[2] = 0.0f;
                splatWeights[3] = 0.0f;

                bool doInterpolation = true;

                splatWeights[0] = splatWeights[1] = splatWeights[2] = splatWeights[3] = 0.0f;
                float h = height;
                if (h > snowThreshold) {
                    splatWeights[3] = 1.0f;
                } else if (h > stoneThreshhold) {
                    splatWeights[2] = 1.0f;
                    if (doInterpolation) {
                        float factor = interp(stoneThreshhold, snowThreshold, h);
                        splatWeights[3] = factor;
                        splatWeights[2] = 1.0f - factor;
                    }
                } else if (h > grassThreshold || h < waterLevel) {
                    splatWeights[1] = 1.0f;
                    if (doInterpolation) {
                        float factor = interp(grassThreshold, stoneThreshhold, h);
                        splatWeights[2] = factor;
                        splatWeights[1] = 1.0f - factor;
                    }
                } else {
                    splatWeights[0] = 1.0f;
                    if (doInterpolation) {
                        float factor = interp(waterLevel, grassThreshold, h);
                        splatWeights[1] = factor;
                        splatWeights[0] = 1.0f - factor;
                    }
                }

                // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
                float z = splatWeights.Sum();

                // Loop through each terrain texture
                for (int i = 0; i < terrainData.alphamapLayers; i++) {
                    // Normalize so that sum of all texture weights = 1
                    splatWeights[i] /= z;
                    // Assign this point to the splatmap array
                    splatmapData[x, y, i] = splatWeights[i];
                }
            }
        }

        // Finally assign the new splatmap to the terrainData:
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

	void Start() {
        RandomizeTerrain();
	}

	public void OnPointerDown(PointerEventData eventData) {
		switch (eventData.button) {
		case PointerEventData.InputButton.Left:
			GameController.instance.StartBoxSelect();
			break;
		case PointerEventData.InputButton.Right:
            GameController.instance.IssueMoveOrder(eventData);
			break;
		}
	}
}
