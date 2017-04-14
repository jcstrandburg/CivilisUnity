using System;
using System.Collections.Generic;
using System.Linq;
using Neolithica.TerrainGeneration;
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

        [SerializeField] private List<ResourceSettings> resourceSettings = new List<ResourceSettings> {
            new ResourceSettings {Type = ResourcePlacementType.Berries, Frequency = 4.0f, Abundance = 0.3f},
            new ResourceSettings {Type = ResourcePlacementType.Trees, Frequency = 1.5f, Abundance = 0.5f},
            new ResourceSettings {Type = ResourcePlacementType.Fish, Frequency = 5.0f, Abundance = 0.2f},
            new ResourceSettings {Type = ResourcePlacementType.Gold, Frequency = 2.0f, Abundance = 0.2f},
            new ResourceSettings {Type = ResourcePlacementType.Stone, Frequency = 2.0f, Abundance = 0.2f},
            new ResourceSettings {Type = ResourcePlacementType.Doodad, Frequency = 10.0f, Abundance = 0.15f},
        };

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

                throw new ArgumentException(string.Format("Invalid value for x {0}", x));
            if (y < 0 || y > 1)
                throw new ArgumentException(string.Format("Invalid value for y {0}", y));

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
            ClearResources();
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
                throw new ArgumentNullException("objects");

            if (maxDist <= 0)
                throw new ArgumentOutOfRangeException("maxDist", maxDist, "Must be positive");

            if (bias < 0)
                throw new ArgumentOutOfRangeException("bias", bias, "Must not be negavite");

            //put the XZ coordinates in a dictionary by InstanceID so that we can tell which object we are comparing with
            float squareMaxDist = maxDist * maxDist;
            var points = new Dictionary<int, Vector2>();
            foreach (var o in objects) {
                var v = new Vector2(o.transform.position.x, o.transform.position.z);
                points[o.GetInstanceID()] = v;
            }

            foreach (var o in objects) {
                var position = new Vector2(o.transform.position.x, o.transform.position.z);
                Vector2 offset = new Vector2(0.0f, 0.0f);

                foreach (var kvp in points) {
                    //get the difference between the position of this object and the camparison object
                    var diff = kvp.Value - position;

                    //if within max distance and not this object
                    if (kvp.Key != o.GetInstanceID() && diff.sqrMagnitude <= squareMaxDist)
                    {
                        float localWeight = 1.0f / diff.magnitude;
                        offset += localWeight * diff;
                    }
                }
                o.transform.position += (new Vector3(offset.x, 0.0f, offset.y))*bias;

                var neolithicObject = o.GetComponent<NeolithicObject>();
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
            return    transform.position
                      + new Vector3(x * terrainData.size.x, 0.0f, y * terrainData.size.z)
                      + Quaternion.Euler(0, 720 * angle, 0) * new Vector3(0, 0, amplitutue * 24);
        }

        private GameObject AttemptPlaceTrees(float x, float y, GameObject prefab, float waterLevel, TerrainData terrainData) {
            float noise = Mathf.PerlinNoise(settings.seed + 9 * x, settings.seed + 9 * y);
            float height = terrainData.GetHeight(
                Mathf.RoundToInt(x * terrainData.heightmapWidth),
                Mathf.RoundToInt(y * terrainData.heightmapHeight));

            if (height > waterLevel && noise > height*settings.treeMultiplier) {
                Vector3 newPosition = RandomizePosition(x, y, terrainData);
                GameObject newTree = GameController.Factory.Instantiate(prefab);
                newTree.transform.position = newPosition;
                newTree.GetComponent<NeolithicObject>().SnapToGround(true);
                return newTree;
            }
            return null;
        }

        private GameObject AttemptPlaceBerries(float x, float y, GameObject prefab, float waterLevel, TerrainData terrainData) {
            float noise = Mathf.PerlinNoise(settings.seed + 17.5f * x, settings.seed + 17.5f * y);
            float height = terrainData.GetHeight(Mathf.RoundToInt(x * terrainData.heightmapWidth),
                Mathf.RoundToInt(y * terrainData.heightmapHeight));
            if (height > waterLevel && noise > height * settings.berryMultiplier) {
                Vector3 newPosition = RandomizePosition(x, y, terrainData);
                GameObject newObject = GameController.Factory.Instantiate(prefab);
                newObject.transform.position = newPosition;
                newObject.GetComponent<NeolithicObject>().SnapToGround(true);
                return newObject;
            }
            return null;
        }

        private GameObject AttemptPlaceStoneOrGold(float x, float y, GameObject[] prefabs, float waterLevel, TerrainData terrainData) {
            float noise = Mathf.PerlinNoise(settings.seed + 29.5f * x, settings.seed*2 + 29.5f * y);
            float height = terrainData.GetHeight(Mathf.RoundToInt(x * terrainData.heightmapWidth), Mathf.RoundToInt(y * terrainData.heightmapHeight));

            if (height > waterLevel && noise < settings.stoneRate) {
                Vector3 newPosition = RandomizePosition(x, y, terrainData);
                GameObject newObject = GameController.Factory.Instantiate(prefabs[mRandom.Next(prefabs.Length - 1)]);
                newObject.transform.position = newPosition;
                newObject.GetComponent<NeolithicObject>().SnapToGround(true);
                return newObject;
            }

            return null;
        }

        private GameObject AttemptPlaceFish(float x, float y, GameObject prefab, float waterLevel, TerrainData terrainData) {
            float noise = Mathf.PerlinNoise(settings.seed*2 + 25.5f * x, settings.seed + 25.5f * y);
            float height = terrainData.GetHeight(Mathf.RoundToInt(x * terrainData.heightmapWidth),
                Mathf.RoundToInt(y * terrainData.heightmapHeight));
            if (height < waterLevel && noise < settings.fishRate) {
                Vector3 newPosition = RandomizePosition(x, y, terrainData);
                GameObject newObject = GameController.Factory.Instantiate(prefab);
                newObject.transform.position = new Vector3(newPosition.x, waterLevel, newPosition.z);
                //newObject.GetComponent<NeolithicObject>().SnapToGround(true);
                return newObject;
            }
            return null;
        }

        private GameObject AttemptPlaceDoodad(float x, float y, GameObject[] prefabs, float waterLevel, TerrainData terrainData) {
            float noise = Mathf.PerlinNoise(settings.seed * 5 + 70f * x, settings.seed * 4 + 70f * y);
            float noise2 = Mathf.PerlinNoise(settings.seed * 6 + 70f * x, settings.seed * 7 + 70f * y);
            float height = terrainData.GetHeight(Mathf.RoundToInt(x * terrainData.heightmapWidth),
                Mathf.RoundToInt(y * terrainData.heightmapHeight));
            if (height > waterLevel && noise < settings.doodadRate) {
                Vector3 newPosition = RandomizePosition(x, y, terrainData);
                int index = (int)(noise2 * prefabs.Length) % prefabs.Length;
                GameObject newObject = GameController.Factory.Instantiate(prefabs[index]);
                newObject.transform.position = newPosition;
                newObject.GetComponent<NeolithicObject>().SnapToGround(true);
                return newObject;
            }
            return null;
        }

        public void ClearResources() {
            Transform resources = transform.Find("Resources");
            Transform doodads = transform.Find("Doodads");

            //we have to store a seperate list to destroy objects since destroying them disrupts the transfrom children iterator
            var gameObjects = new List<GameObject>();
            foreach (Transform t in resources.transform) {
                gameObjects.Add(t.gameObject);
            }
            foreach (Transform t in doodads.transform) {
                gameObjects.Add(t.gameObject);
            }

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
            ClearResources();

            var prefabs = new Dictionary<ResourcePlacementType, List<GameObject>>();

            // TODO: get rid of this water junk
            var water = GameObject.Find("Water4Simple");

            GameObject[] doodadPrefabs = {
                (GameObject)Resources.Load("Doodads/DeadTree6"),
                (GameObject)Resources.Load("Doodads/DeadTree7"),
                (GameObject)Resources.Load("Doodads/Smallbush4"),
                (GameObject)Resources.Load("Doodads/SmallRock4"),
                (GameObject)Resources.Load("Doodads/SmallRock7"),
            };

            prefabs[ResourcePlacementType.Trees] = new List<GameObject> { Resources.Load<GameObject>("Buildings/WoodSource") };
            prefabs[ResourcePlacementType.Stone] = new List<GameObject> { Resources.Load<GameObject>("Buildings/StoneRocks") };
            prefabs[ResourcePlacementType.Gold] = new List<GameObject> { Resources.Load<GameObject>("Buildings/GoldRocks") };
            prefabs[ResourcePlacementType.Fish] = new List<GameObject> { Resources.Load<GameObject>("Prefabs/FishingHole") };
            prefabs[ResourcePlacementType.Berries] = new List<GameObject> { Resources.Load<GameObject>("Buildings/ForagingGround") };
            prefabs[ResourcePlacementType.Doodad] = doodadPrefabs.ToList();

            Debug.Log(prefabs[ResourcePlacementType.Fish]);
            Debug.Log(prefabs[ResourcePlacementType.Fish][0]);

            var terrainData = GetComponent<Terrain>().terrainData;
            float waterLevel = water.transform.position.y;
            float waterHeight = waterLevel / terrainData.size.y;

            var placer = new ResourcePlacer(terrainData, waterHeight, settings, resourceSettings);
            List<GameObject> trees = new List<GameObject>();
            List<GameObject> berries = new List<GameObject>();

            const int resolution = 45;
            for (int x = 0; x < resolution; ++x) {
                for (int y = 0; y < resolution; ++y) {
                    float u = (x + 0.5f) / resolution;
                    float v = (y + 0.5f) / resolution;

                    var type = placer.GetPlacementType(u, v);

                    switch (type) {
                        case ResourcePlacementType.None:
                            break;
                        default:
                            int index = mRandom.Next(0, prefabs[type].Count);
                            var prefab = prefabs[type][index];

                            if (prefab == null) {
                                Debug.Log(type);
                                Debug.Log(index);
                            }

                            var instance = GameController.Factory.Instantiate(prefab);
                            instance.transform.position = RandomizePosition(u, v, terrainData);

                            if (type == ResourcePlacementType.Fish)
                                instance.transform.position = new Vector3(
                                    instance.transform.position.x,
                                    waterLevel,
                                    instance.transform.position.z);
                            else
                                instance.GetComponent<NeolithicObject>().SnapToGround();

                            if (type == ResourcePlacementType.Trees)
                                trees.Add(instance);
                            else if (type == ResourcePlacementType.Berries)
                                berries.Add(instance);

                            if (type == ResourcePlacementType.Doodad)
                                instance.transform.SetParent(doodads);
                            else
                                instance.transform.SetParent(resources);
                            break;
                    }
                }
            }

            ClusterResources(trees, 200.0f, 3.0f);
            ClusterResources(berries, 400.0f, 12.0f);
        }

        private void GenerateSplatMap() {
            Terrain terrain = GetComponent<Terrain>();
            TerrainData terrainData = terrain.terrainData;
            const bool c_doInterpolation = true;

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

                    splatWeights[0] = splatWeights[1] = splatWeights[2] = splatWeights[3] = 0.0f;
                    float h = height;
                    if (h > snowThreshold) {
                        splatWeights[3] = 1.0f;
                    } else if (h > stoneThreshhold) {
                        splatWeights[2] = 1.0f;
                        if (c_doInterpolation) {
                            float factor = Interpolate(stoneThreshhold, snowThreshold, h);
                            splatWeights[3] = factor;
                            splatWeights[2] = 1.0f - factor;
                        }
                    } else if (h > grassThreshold || h < waterLevel) {
                        splatWeights[1] = 1.0f;
                        if (c_doInterpolation) {
                            float factor = Interpolate(grassThreshold, stoneThreshhold, h);
                            splatWeights[2] = factor;
                            splatWeights[1] = 1.0f - factor;
                        }
                    } else {
                        splatWeights[0] = 1.0f;
                        if (c_doInterpolation) {
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
            default:
                // do nothing
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
