using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Neolithica.MonoBehaviours {
    public class GroundController : MonoBehaviour, IPointerDownHandler {
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

        public float treeThinning = 150.0f;
        public float treeMultiplier = 1 / 100.0f;
        public float berryMultiplier = 1 / 150.0f;
        public float stoneRate = 0.17f;
        public float fishRate = 0.2f;
        public float doodadRate = 0.35f;

        private List<GameObject> trees = new List<GameObject>();
        private List<GameObject> berries = new List<GameObject>();

        [Inject]
        public GameController GameController { get; set; }

        public void ApplySettings(NewGameSettings settings) {
            this.floatSeed = settings.seed;
            this.treeMultiplier = settings.treeMultiplier;
            this.berryMultiplier = settings.berryMultiplier;
            this.stoneRate = settings.stoneRate;
            this.fishRate = settings.fishRate;
            this.doodadRate = settings.doodadRate;       
        }

        [Obsolete("This should come from new game settings now")]
        public void RandomizeSeed() {
            floatSeed = UnityEngine.Random.Range(0.0f, 100.0f);
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

        /// <summary>
        /// Generates terrain mesh, textures, and resource objects
        /// </summary>
        public void GenerateMap() {
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
            GenerateResources();

            ClusterResources(trees, 200.0f, 3.0f);
            ClusterResources(berries, 400.0f, 12.0f);
        }

        /// <summary>
        /// Clusters the given resources together but averaging the positions of nearby resources
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="maxDist">The maximum distance on the XZ plane to consider objects at</param>
        /// <param name="bias"></param>
        public void ClusterResources(IEnumerable<GameObject> objects, float maxDist, float bias) {

            //put the XZ coordinates in a dictionary by InstanceID so that we
            //can tell which object we are comparing with
            float squareMaxDist = maxDist * maxDist;
            var points = new Dictionary<int, Vector2>();
            foreach (var o in objects) {
                var v = new Vector2(o.transform.position.x, o.transform.position.z);
                points[o.GetInstanceID()] = v;
            }

            foreach (var o in objects) {
                float weight = 0.0f;
                var position = new Vector2(o.transform.position.x, o.transform.position.z);
                Vector2 offset = new Vector2(0.0f, 0.0f);

                foreach (var kvp in points) {
                    //get the difference between the position of this object and the camparison object
                    var diff = kvp.Value - position;

                    //if within max distance and not this object
                    if (kvp.Key != o.GetInstanceID() 
                        && diff.sqrMagnitude <= squareMaxDist) 
                    {
                        float localWeight = 1.0f / diff.magnitude;
                        weight += localWeight;
                        offset += localWeight * diff;
                    }
                }
                o.transform.position += (new Vector3(offset.x, 0.0f, offset.y))*bias;
                if (o.GetComponent<NeolithicObject>()) {
                    o.GetComponent<NeolithicObject>().SnapToGround(true);
                }
            }
        }
    

        public float interp(float a, float b, float c) {
            float t = (c - a) / (b - a);//linear interpolation

            if (cosineTerrain) {
                t = 0.5f - Mathf.Cos(Mathf.PI * t) / 2.0f;//convert to cosine interpolation
            }

            return t;
        }

        private Vector3 randomizePosition(float x, float y, TerrainData terrainData) {
            float angle = Mathf.PerlinNoise(floatSeed + 65 * x, floatSeed + 65 * y);
            float amplitutue = Mathf.PerlinNoise(floatSeed + 25 * x, floatSeed + 25 * y);
            return    transform.position
                      + new Vector3(x * terrainData.size.x, 30.0f, y * terrainData.size.z)
                      + Quaternion.Euler(0, 720 * angle, 0) * new Vector3(0, 0, amplitutue * 24);
        }

        private GameObject AttemptPlaceTrees(float x, float y, GameObject prefab, float waterLevel, TerrainData terrainData) {
            float noise = Mathf.PerlinNoise(floatSeed + 9 * x, floatSeed + 9 * y);
            float height = terrainData.GetHeight(Mathf.RoundToInt(x * terrainData.heightmapWidth),
                Mathf.RoundToInt(y * terrainData.heightmapHeight));
            if (height > waterLevel && noise > height*treeMultiplier) {
                Vector3 newPosition = randomizePosition(x, y, terrainData);
                GameObject newTree = GameController.Factory.Instantiate(prefab);
                newTree.transform.position = newPosition;
                newTree.GetComponent<NeolithicObject>().SnapToGround(true);
                return newTree;
            }
            return null;
        }

        private GameObject AttemptPlaceBerries(float x, float y, GameObject prefab, float waterLevel, TerrainData terrainData) {
            float noise = Mathf.PerlinNoise(floatSeed + 17.5f * x, floatSeed + 17.5f * y);
            float height = terrainData.GetHeight(Mathf.RoundToInt(x * terrainData.heightmapWidth),
                Mathf.RoundToInt(y * terrainData.heightmapHeight));
            if (height > waterLevel && noise > height * berryMultiplier) {
                Vector3 newPosition = randomizePosition(x, y, terrainData);
                GameObject newObject = GameController.Factory.Instantiate(prefab);
                newObject.transform.position = newPosition;
                newObject.GetComponent<NeolithicObject>().SnapToGround(true);
                return newObject;
            }
            return null;
        }

        private GameObject AttemptPlaceStoneOrGold(float x, float y, GameObject[] prefabs, float waterLevel, TerrainData terrainData) {
            float noise = Mathf.PerlinNoise(floatSeed + 29.5f * x, floatSeed*2 + 29.5f * y);
            float noise2 = Mathf.PerlinNoise(floatSeed*2 + 29.5f * x, floatSeed * 3 + 29.5f * y);
            float height = terrainData.GetHeight(Mathf.RoundToInt(x * terrainData.heightmapWidth),
                Mathf.RoundToInt(y * terrainData.heightmapHeight));
            if (height > waterLevel && noise < stoneRate) {
                Vector3 newPosition = randomizePosition(x, y, terrainData);
                int index = (int)(noise2 * prefabs.Length)%prefabs.Length;
                GameObject newObject = GameController.Factory.Instantiate(prefabs[index]);
                newObject.transform.position = newPosition;
                newObject.GetComponent<NeolithicObject>().SnapToGround(true);
                return newObject;
            }
            return null;
        }

        private GameObject AttemptPlaceFish(float x, float y, GameObject prefab, float waterLevel, TerrainData terrainData) {
            float noise = Mathf.PerlinNoise(floatSeed*2 + 25.5f * x, floatSeed + 25.5f * y);
            float height = terrainData.GetHeight(Mathf.RoundToInt(x * terrainData.heightmapWidth),
                Mathf.RoundToInt(y * terrainData.heightmapHeight));
            if (height < waterLevel && noise < fishRate) {
                Vector3 newPosition = randomizePosition(x, y, terrainData);
                GameObject newObject = GameController.Factory.Instantiate(prefab);
                newObject.transform.position = new Vector3(newPosition.x, waterLevel, newPosition.z);
                //newObject.GetComponent<NeolithicObject>().SnapToGround(true);
                return newObject;
            }
            return null;
        }

        private GameObject AttemptPlaceDoodad(float x, float y, GameObject[] prefabs, float waterLevel, TerrainData terrainData) {
            float noise = Mathf.PerlinNoise(floatSeed * 5 + 70f * x, floatSeed * 4 + 70f * y);
            float noise2 = Mathf.PerlinNoise(floatSeed * 6 + 70f * x, floatSeed * 7 + 70f * y);
            float height = terrainData.GetHeight(Mathf.RoundToInt(x * terrainData.heightmapWidth),
                Mathf.RoundToInt(y * terrainData.heightmapHeight));
            if (height > waterLevel && noise < doodadRate) {
                Vector3 newPosition = randomizePosition(x, y, terrainData);
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

            trees.Clear();
            berries.Clear();
        }


        public void GenerateResources() {
            Transform resources = transform.Find("Resources");
            Transform doodads = transform.Find("Doodads");
            ClearResources();

            //load prefabs
            GameObject water = GameObject.Find("Water4Simple");
            GameObject tree = (GameObject)Resources.Load("Buildings/WoodSource");
            GameObject stoneRocks = (GameObject)Resources.Load("Buildings/StoneRocks");
            GameObject goldRocks = (GameObject)Resources.Load("Buildings/GoldRocks");
            GameObject fish = (GameObject)Resources.Load("Prefabs/FishingHole");
            GameObject berriesPrefab = (GameObject)Resources.Load("Buildings/ForagingGround");

            GameObject[] doodadPrefabs = new GameObject[] {
                (GameObject)Resources.Load("Doodads/DeadTree6"),
                (GameObject)Resources.Load("Doodads/DeadTree7"),
                (GameObject)Resources.Load("Doodads/Smallbush4"),
                (GameObject)Resources.Load("Doodads/SmallRock4"),
                (GameObject)Resources.Load("Doodads/SmallRock7"),
            };

            float waterLevel = water.transform.position.y;
            TerrainData terrainData = GetComponent<Terrain>().terrainData;
            int resolution = 45;
            for (int x = 0; x < resolution; ++x) {
                for (int y = 0; y < resolution; ++y) {
                    float x1 = (0.5f + x) / resolution;
                    float y1 = (0.5f + y) / resolution;

                    GameObject newObject;
                    if ((newObject = AttemptPlaceTrees(x1, y1, tree, waterLevel, terrainData)) != null) {
                        newObject.transform.SetParent(resources);
                        trees.Add(newObject);
                    } else if ((newObject = AttemptPlaceBerries(x1, y1, berriesPrefab, waterLevel, terrainData)) != null) {
                        newObject.transform.SetParent(resources);
                        berries.Add(newObject);
                    } else if ((newObject = AttemptPlaceStoneOrGold(x1, y1, new GameObject[] {stoneRocks, goldRocks}, waterLevel, terrainData)) != null) {
                        newObject.transform.SetParent(resources);
                    } else if ((newObject = AttemptPlaceFish(x1, y1, fish, waterLevel, terrainData)) != null) {
                        newObject.transform.SetParent(resources);
                    } else if ((newObject = AttemptPlaceDoodad(x1, y1, doodadPrefabs, waterLevel, terrainData)) != null) {
                        newObject.transform.SetParent(doodads);
                    }
                }
            }
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
            //RandomizeTerrain();
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
    }
}
