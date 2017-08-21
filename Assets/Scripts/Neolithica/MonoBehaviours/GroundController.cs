using System;
using System.Collections.Generic;
using System.Linq;
using Neolithica.TerrainGeneration;
using Neolithica.Utility;
using Tofu.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = System.Random;

namespace Neolithica.MonoBehaviours {
    [SavableMonobehaviour(25)]
    public class GroundController : MonoBehaviour, IPointerDownHandler {
        public TerrainSettings terrainSettings;
        public EroderSettings eroderSettings;
        public NewGameSettings settings;

        // TODO: Fix this junk
        public float snowThreshold = 150.0f;
        public float stoneThreshhold = 130.0f;
        public float grassThreshold = 105.0f;
        public float waterLevel = 20.0f;

        private Random mRandom = new Random();
        private float[,] heights;

        [Inject]
        public GameController GameController { get; set; }

        public void ApplySettings(NewGameSettings settings) {
            this.settings = settings;
        }

        /// <summary>
        /// Gets prodecural terrain height at the given coords
        /// </summary>
        /// <param name="x">(0...1)</param>
        /// <param name="y">(0...1)</param>
        /// <returns></returns>
        protected float GetHeight(float x, float y) {
            if (x < 0 || x > 1)

                throw new ArgumentException($"Invalid value for x {x}", nameof(x));
            if (y < 0 || y > 1)
                throw new ArgumentException($"Invalid value for y {y}", nameof(y));

            float xOffset = settings.seed * 1.2f;
            float yOffset = settings.seed * 0.9f;
            float dist = Mathf.Min(1.0f, Mathf.Sqrt((x - 0.5f) * (x - 0.5f) + (y - 0.5f) * (y - 0.5f))); //normalized distance from center in range 0..1.414

            float hHeight = (terrainSettings.hillBaseOffset + Mathf.PerlinNoise(xOffset + x * terrainSettings.hillFrequency, yOffset + y * terrainSettings.hillFrequency)) * terrainSettings.hillHeight;
            float bHeight = Mathf.PerlinNoise(xOffset + x * terrainSettings.bumpFrequency, yOffset + y * terrainSettings.bumpFrequency) * terrainSettings.bumpHeight;

            float rHeight = terrainSettings.riverHeight * Mathf.Pow(2*Mathf.Abs(Mathf.PerlinNoise(xOffset + x * terrainSettings.riverFrequency, yOffset + y * terrainSettings.riverFrequency) - 0.5f), terrainSettings.riverExponent);
            float mHeight = terrainSettings.mountainHeight * Mathf.Pow(Mathf.PerlinNoise(xOffset + x * terrainSettings.mountainFrequency, yOffset + y * terrainSettings.mountainFrequency), terrainSettings.mountainSharpness) * Mathf.Pow(dist, terrainSettings.mountainFalloff);

            return Mathf.Min(mHeight+hHeight, rHeight)+bHeight;
        }

        /// <summary>
        /// Generates terrain mesh, textures, and resource objects
        /// </summary>
        public void GenerateMap() {
            Terrain terrain = GetComponent<Terrain>();
            terrain.terrainData = Instantiate(terrain.terrainData);

            GetComponent<TerrainCollider>().terrainData = terrain.terrainData;

            heights = new float[terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight];

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

        /// <summary>
        /// Clears current resources (and doodads) and regenerates new ones
        /// </summary>
        public void GenerateResourcesAndDoodads() {
            mRandom = new Random(settings.seed.GetHashCode());
            SpawnResourcesAndDoodads();
        }

        /// <summary>
        /// Clusters the given resources together but averaging the positions of nearby resources
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="maxDist">The maximum distance on the XZ plane to consider objects at</param>
        /// <param name="bias">The bias towards the center of the local cluster. Higher values will lead to tighter clustering.</param>
        public void ClusterResources(ICollection<GameObject> objects, float maxDist, float bias) {

            if (objects == null)
                throw new ArgumentNullException(nameof(objects));

            if (maxDist <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxDist), maxDist, "Must be positive");

            if (bias < 0)
                throw new ArgumentOutOfRangeException(nameof(bias), bias, "Must not be negavite");

            //put the XZ coordinates in a dictionary by InstanceID so that we can tell which object we are comparing with
            float squareMaxDist = maxDist * maxDist;
            var points = new Dictionary<int, Vector2>();
            foreach (var o in objects) {
                var v = new Vector2(o.transform.position.x, o.transform.position.z);
                points[o.GetInstanceID()] = v;
            }

            foreach (GameObject obj in objects) {
                var position = new Vector2(obj.transform.position.x, obj.transform.position.z);
                var offset = new Vector2(0.0f, 0.0f);

                foreach (var kvp in points) {
                    //get the difference between the position of this object and the camparison object
                    Vector2 diff = kvp.Value - position;

                    //if within max distance and not this object
                    if (kvp.Key != obj.GetInstanceID() && diff.sqrMagnitude <= squareMaxDist)
                    {
                        float localWeight = 1.0f / diff.magnitude;
                        offset += localWeight * diff;
                    }
                }
                obj.transform.position += (new Vector3(offset.x, 0.0f, offset.y))*bias;

                var neolithicObject = obj.GetComponent<NeolithicObject>();
                if (neolithicObject != null) {
                    neolithicObject.SnapToGround(true);
                }
            }
        }

        private float Interpolate(float a, float b, float c) {
            float t = (c - a) / (b - a);//linear interpolation

            if (terrainSettings.cosineTerrain) {
                t = 0.5f - Mathf.Cos(Mathf.PI * t) / 2.0f;//convert to cosine interpolation
            }

            return t;
        }

        // todo this can use unity's random unit circle function
        private Vector3 RandomizePosition(float x, float y, TerrainData terrainData) {
            float angle = Mathf.PerlinNoise(settings.seed + 65 * x, settings.seed + 65 * y);
            float amplitutue = Mathf.PerlinNoise(settings.seed + 25 * x, settings.seed + 25 * y);
            return transform.position
                + new Vector3(x * terrainData.size.x, 0.0f, y * terrainData.size.z)
                + Quaternion.Euler(0, 720 * angle, 0) * new Vector3(0, 0, amplitutue * 24);
        }

        public void ClearResources(Transform resources, Transform doodads) {
            //we have to store a seperate list to destroy objects since destroying them disrupts the transfrom children iterator
            var gameObjects = (from Transform t in resources.transform select t.gameObject)
                .Concat(from Transform t in doodads.transform select t.gameObject)
                .ToList();

            foreach (GameObject go in gameObjects) {
#if UNITY_EDITOR
                DestroyImmediate(go);
#else
                Destroy(go);
#endif
            }
        }

        private void SpawnResourcesAndDoodads() {
            Transform resources = transform.Find("Resources");
            Transform doodads = transform.Find("Doodads");
            ClearResources(resources, doodads);

            // TODO: get rid of this water junk
            GameObject water = GameObject.Find("Water4Simple");

            GameObject[] doodadPrefabs = {
                (GameObject)Resources.Load("Doodads/DeadTree6"),
                (GameObject)Resources.Load("Doodads/DeadTree7"),
                (GameObject)Resources.Load("Doodads/Smallbush4"),
                (GameObject)Resources.Load("Doodads/SmallRock4"),
                (GameObject)Resources.Load("Doodads/SmallRock7"),
            };


            var prefabs = new Dictionary<ResourcePlacementType, List<GameObject>>
            {
                [ResourcePlacementType.Trees]   = ListUtility.From(Resources.Load<GameObject>("Buildings/WoodSource")),
                [ResourcePlacementType.Stone]   = ListUtility.From(Resources.Load<GameObject>("Buildings/StoneRocks")),
                [ResourcePlacementType.Gold]    = ListUtility.From(Resources.Load<GameObject>("Buildings/GoldRocks")),
                [ResourcePlacementType.Fish]    = ListUtility.From(Resources.Load<GameObject>("Prefabs/FishingHole")),
                [ResourcePlacementType.Berries] = ListUtility.From(Resources.Load<GameObject>("Buildings/ForagingGround")),
                [ResourcePlacementType.Doodad]  = doodadPrefabs.ToList(),
            };

            TerrainData terrainData = GetComponent<Terrain>().terrainData;
            float waterLevel = water.transform.position.y;
            float waterHeight = waterLevel / terrainData.size.y;

            ResourcePlacer placer = new ResourcePlacer(terrainData, waterHeight, settings, settings.ResourceSettings);
            List<GameObject> trees = new List<GameObject>();
            List<GameObject> berries = new List<GameObject>();

            const int resolution = 45;
            for (int x = 0; x < resolution; ++x) {
                for (int y = 0; y < resolution; ++y) {
                    float u = (x + 0.5f) / resolution;
                    float v = (y + 0.5f) / resolution;

                    ResourcePlacementType type = placer.GetPlacementType(u, v);

                    switch (type) {
                        case ResourcePlacementType.None:
                            break;
                        default:
                            int index = mRandom.Next(0, prefabs[type].Count);
                            GameObject prefab = prefabs[type][index];

                            GameObject instance = GameController.Factory.Instantiate(prefab);
                            instance.transform.position = RandomizePosition(u, v, terrainData);

                            if (type == ResourcePlacementType.Fish)
                                instance.transform.position = new Vector3(
                                    instance.transform.position.x,
                                    waterLevel,
                                    instance.transform.position.z);
                            else
                                instance.GetComponent<NeolithicObject>().SnapToGround();

                            switch (type)
                            {
                                case ResourcePlacementType.Trees:
                                    trees.Add(instance);
                                    break;
                                case ResourcePlacementType.Berries:
                                    berries.Add(instance);
                                    break;
                            }

                            instance.transform.SetParent(type == ResourcePlacementType.Doodad ? doodads : resources);
                            break;
                    }
                }
            }

            ClusterResources(trees, 200.0f, 3.0f);
            ClusterResources(berries, 400.0f, 12.0f);
        }

        private void GenerateSplatMap() {
            var terrain = GetComponent<Terrain>();
            TerrainData terrainData = terrain.terrainData;
            const bool cDoInterpolation = true;

            var splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

            for (var y = 0; y < terrainData.alphamapHeight; y++) {
                for (var x = 0; x < terrainData.alphamapWidth; x++) {
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

                    splatWeights[0] = splatWeights[1] = splatWeights[2] = splatWeights[3] = 0.0f;
                    float h = height;
                    if (h > snowThreshold) {
                        splatWeights[3] = 1.0f;
                    } else if (h > stoneThreshhold) {
                        splatWeights[2] = 1.0f;
                        if (cDoInterpolation) {
                            float factor = Interpolate(stoneThreshhold, snowThreshold, h);
                            splatWeights[3] = factor;
                            splatWeights[2] = 1.0f - factor;
                        }
                    } else if (h > grassThreshold || h < waterLevel) {
                        splatWeights[1] = 1.0f;
                        if (cDoInterpolation) {
                            float factor = Interpolate(grassThreshold, stoneThreshhold, h);
                            splatWeights[2] = factor;
                            splatWeights[1] = 1.0f - factor;
                        }
                    } else {
                        splatWeights[0] = 1.0f;
                        if (cDoInterpolation) {
                            float factor = Interpolate(waterLevel, grassThreshold, h);
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

        public void OnPointerDown(PointerEventData eventData) {
            switch (eventData.button) {
            case PointerEventData.InputButton.Left:
                GameController.StartBoxSelect();
                break;
            case PointerEventData.InputButton.Right:
                GameController.IssueMoveOrder(eventData);
                break;
            }
        }

        // Handles Start event
        public void Awake() {
            GenerateMap();
        }

        public void ErodeMap() {
            var eroder = new Eroder(heights, eroderSettings);
            foreach (int i in Enumerable.Range(0, 5000))
                eroder.Erode();

            var smoother = new Smoother(heights);
            heights = smoother.Smooth(4, 1.0f);
            GetComponent<Terrain>().terrainData.SetHeights(0, 0, heights);
        }

        public void Randomize() {
            settings.seed = (float) mRandom.NextDouble()*1000.0f;
        }
    }
}
