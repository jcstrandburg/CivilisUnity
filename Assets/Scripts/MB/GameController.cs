using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using System;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(GameController))]
public class GameControllerEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        GameController gc = (GameController)target;
        if (GUILayout.Button("TestResources")) {
            var x = gc.GetAllAvailableResources();
            foreach (var y in x) {
                Debug.Log(y.Key + " " + y.Value);
            }
        }
    }
}

#endif

public class GameController : MonoBehaviour {
	public List<NeolithicObject> selected = new List<NeolithicObject>();
	public GameObject mainLight;
	Vector2 boxStart;
	Vector2 boxEnd;
	public bool boxActive = false;
	public bool additiveSelect = false;
    public BuildingBlueprint _buildingPlacer;
    public List<Resource> resourcePrefabs;
    public TechManager techmanager;

    [SerializeField]
    private float _foodbuffer = 6.0f;
    private float _spirit = 0.0f;
    private GameUIController guiController;

    private static GameObject _object = null;
    private static GameController _instance = null;

	public static GameController instance {
		get {
			if (_object == null || _instance == null) {
				_object = GameObject.Find("_Scripts");
				_instance = _object.GetComponent<GameController>();
			}
			return _instance;
		}
	}

    private MenuManager menuManager {
        get {
            var go = GameObject.Find("MenuManager");
            return go.GetComponent<MenuManager>();
        }
    }

    public float foodbuffer {
        get {
            return _foodbuffer;
        }
        set {
            _foodbuffer = value;
        }
    }

    public float spirit {
        get {
            return _spirit;
        }
        set {
            _spirit = value;
        }
    }

    private BuildingBlueprint buildingPlacer {
        get {
            if (_buildingPlacer == null) {
                BuildingBlueprint[] bbps = FindObjectsOfType<BuildingBlueprint>();
                if (bbps.Length > 0) {
                    _buildingPlacer = bbps[0];
                }
            }
            return _buildingPlacer;
        }
    }

    public void EndGame() {
        Application.Quit();
    }

    // Use this for initialization
    void Start () {
        UnityEngine.Object[] objects = Resources.LoadAll("Techs", typeof(TextAsset));
        guiController = GameObject.Find("GameUI").GetComponent<GameUIController>();
        string[] jsonText = Array.ConvertAll(objects, (x) => ((TextAsset)x).text);
        techmanager = new TechManager();
        techmanager.LoadTree(jsonText);

        resourcePrefabs = new List<Resource>();
        var allFiles = Resources.LoadAll<UnityEngine.Object>("");
        foreach (var obj in allFiles) {
            if (obj is GameObject) {
                if (((GameObject)obj).GetComponent<Resource>() != null) {
                    resourcePrefabs.Add(((GameObject)obj).GetComponent<Resource>());
                }
            }
        }
        InvokeRepeating("KeepFoodBufferFilled", 1.0f, 0.5f);
    }

    void OnDeserialize() {
        selected.Clear();
    }

    public Technology[] GetAvailableTechs() {
        return techmanager.GetEligibleTechs();
    }

    public void BuyTech(Technology t) {
        Debug.Log("Researching tech: " + t.name);
		if (t.cost <= this.spirit) {
			this.spirit -= t.cost;
			techmanager.Research(t);
		}
        GameUIController.instance.subMenu.ClearMenu();
    }

    /// <summary>
    /// Takes a given world position and returns it after changing it's y component to reflect the y position of the terrain at those x and z coords
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Vector3 SnapToGround(Vector3 pos) {
        if (Terrain.activeTerrain) {
            float y = Terrain.activeTerrain.transform.position.y + Terrain.activeTerrain.SampleHeight(pos);
            Vector3 returnMe = pos;
            returnMe.y = y;
            return returnMe;
        } else {
            return pos;
        }
    }

    public void PauseGame() {
        Time.timeScale = 0.0f;
        menuManager.PushMenuName("PauseMenu");
    }

    public void UnpauseGame() {
        Time.timeScale = 1.0f;
    }

    /// <summary>
    /// This function appears to be broken atm, don't use until it gets debugged more betterer
    /// </summary>
    /// <param name="pos">The world position we want to get the normal for. This point is not necessarily on the ground, only the x and z are used</param>
    /// <returns>The ground normal vector below the given position</returns>
    public Vector3 GetGroundNormal(Vector3 pos) {
        Terrain t = Terrain.activeTerrain;
        TerrainData td = t.terrainData;
        Vector3 terrainLocal = pos - t.transform.position;
        Vector2 normalizedPos = new Vector2(Mathf.InverseLerp(0.0f, td.size.x, terrainLocal.x),
                                            Mathf.InverseLerp(0.0f, td.size.y, terrainLocal.z));
        Vector3 normal = td.GetInterpolatedNormal(normalizedPos.x, normalizedPos.y);
        return normal;
    }

    /// <summary>
    /// Calculates the value of a given collection of food resources, 
    /// with increasing value for greater variety of food types
    /// </summary>
    /// <param name="resources"></param>
    /// <returns>Food value</returns>
    public float CalcFoodValue(IEnumerable<ResourceProfile> resources) {
        var p = resources.OrderBy((ResourceProfile rp) => -rp.amount).ToArray();
        if (p.Count() == 0 || p.Count() > 3) {
            throw new ArgumentException("Unexpected resource count " + resources.Count());
        }

        float returnMe = 0.0f;
        for (int i = 0; i < p.Length; i++) {
            returnMe += (i + 1) * p[i].amount;
        }
        return returnMe;
    }

    /// <summary>
    /// Run in the background via InvokeRepeating, attempts to consume 
    /// food from any source to keep the food buffer filled
    /// </summary>
    /// <todo>Rework to use a logistics system</todo>
    void KeepFoodBufferFilled() {
        if (foodbuffer < 3.0f) {
            var warehouses = FindObjectsOfType<Warehouse>();
            var tags = new List<string> { "meat", "vegetables", "fish" };
            var tagsToRemove = new List<string>();
            var resources = new List<ResourceProfile>();

            foreach (var w in warehouses) {
                foreach (var t in tags) {
                    if (w.GetAvailableContents(t) >= 1.0f) {
                        tagsToRemove.Add(t);
                        w.WithdrawContents(t, 1.0f);
                        resources.Add(new ResourceProfile(t, 1.0f));
                    }
                }
                foreach (var t in tagsToRemove) {
                    tags.Remove(t);
                }
                tagsToRemove.Clear();

                if (tags.Count == 0) {
                    break;
                }
            }

            if (resources.Count > 0) {
                foodbuffer += CalcFoodValue(resources);
            }
        }
    }


    public bool WithdrawFromAnyWarehouse(ResourceProfile rp) {
        var warehouses = FindObjectsOfType<Warehouse>();
        foreach (var w in warehouses) {
            float amount = Mathf.Min(rp.amount, w.GetAvailableContents(rp.resourceTag));
            if (amount > 0) {
                w.WithdrawContents(rp.resourceTag, amount);
                rp.amount -= amount;
            }

            if (rp.amount <= 0.0f) {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Gets all available contents in all warehouses in game total
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, float> GetAllAvailableResources() {
        var d = new Dictionary<string, float>();
        var warehouses = FindObjectsOfType<Warehouse>();
        foreach (var w in warehouses) {
            var x = w.GetAllAvailableContents();
            foreach (var kvp in x) {
                if (d.ContainsKey(kvp.Key)) {
                    d[kvp.Key] += kvp.Value;
                } else {
                    d[kvp.Key] = kvp.Value;
                }
            }
        }
        return d;
    }

	void FixedUpdate() {
        //remove destroyed objects from selection list
        selected.RemoveAll((s) => (s == null));

		//mainLight.transform.eulerAngles += new Vector3(0, 0.2f, 0);
		additiveSelect = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

	public void AddSelected(NeolithicObject sel) {
		guiController.HideContextMenu();
		if (!selected.Contains(sel)) {
			selected.Add(sel);
		}
		if (selected.Count == 1) {
			guiController.ShowSelectionMenu(sel);
		} else {
			guiController.HideSelectionMenu();
		}
	}

    public void StartBuildingPlacement(GameObject prefab) {
        GameUIController.instance.subMenu.ClearMenu();
        buildingPlacer.Activate(prefab);
    }

    private GameObject singleMouseCast(int layerMask) {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(Camera.main.transform.position, ray.direction, out hit, Mathf.Infinity, layerMask)) {
			return hit.collider.gameObject;
		} else {
			return default(GameObject);
		}
	}

	private string[] getSelectedAbilities() {
		IEnumerable<string> sharedAbilities = null;

		if (selected.Count == 0) {
			return new string[] {};
		}
		foreach (NeolithicObject s in selected) {
			if (sharedAbilities == null) {
				sharedAbilities = s.abilities;
			} else {
				sharedAbilities = s.abilities.Intersect(sharedAbilities);
			}
		}
		return sharedAbilities.ToArray();
	}

    public GameObject[] GetBuildableBuildings() {
        UnityEngine.Object[] objects = Resources.LoadAll("Buildings", typeof(ConstructionManager));
        var c = from o in objects select ((ConstructionManager)o).gameObject;
        return c.ToArray();
    }

	public void StartBoxSelect() {
		guiController.HideContextMenu();
		boxEnd = boxStart = Input.mousePosition;
		boxActive = true;
	}

	private List<NeolithicObject> GetWithinBoxSelect() {
		return null;
	}

	public void UpdateBoxSelect() {
		boxEnd = Input.mousePosition;
		NeolithicObject[] allObjects = FindObjectsOfType(typeof(NeolithicObject)) as NeolithicObject[];
		Vector2 start = boxStart;
		Vector2 end = boxEnd;
		
		if ( end.x < start.x) {
			float temp = end.x;
			end = new Vector2(start.x, end.y);
			start = new Vector2(temp, start.y);
		}
		if ( end.y > start.y ) {
			float temp = end.y;
			end = new Vector2(end.x, start.y);
			start = new Vector2(start.x, temp);
		}
		Rect r = new Rect (start.x, Screen.height-start.y, end.x-start.x, start.y-end.y);

		List<NeolithicObject> selectables = new List<NeolithicObject>();
		foreach (NeolithicObject no in allObjects) {
			Vector2 loc = Camera.main.WorldToScreenPoint(no.transform.position);
			loc = new Vector2(loc.x, Screen.height-loc.y);
			no.HoverEnd();
			if (r.Contains(loc)) {
				selectables.Add(no);
			}
		}
		bool restrictToMultiselect = (selectables.Count > 1) || (additiveSelect && (selected.Count + selectables.Count) > 1);
		if (restrictToMultiselect) {
			selectables.RemoveAll(obj => obj.selectability != NeolithicObject.Selectability.Multiselectable);
		}

		foreach (NeolithicObject obj in selectables) {
			obj.HoverStart();
		}

		if (!Input.GetMouseButton(0)) {
			EndBoxSelect(selectables);
		}
	}

	public void EndBoxSelect(List<NeolithicObject> selectables) {
		boxActive = false;
		if (!additiveSelect) {
			DeselectAll();
		}
		foreach (NeolithicObject no in selectables) {
			no.HoverEnd();
			no.Select();
		}
	}

	public void DoContextMenu(NeolithicObject clickee) {
		string[] selectedActions = getSelectedAbilities();
		string[] availableActions = selectedActions.Intersect(clickee.targetActions).ToArray();
		//Debug.Log(string.Join(", ", availableActions));
		guiController.ShowContextMenu(availableActions, clickee);
	}

	/// <summary>Constructs an order from the orderTag against the given target, and assigns it to all selected actors</summary>
	public void IssueOrder(string orderTag, NeolithicObject target) {
		foreach (NeolithicObject s in selected) {
			ActorController a = s.GetComponent<ActorController>();
			if (a) {
				BaseOrder newOrder = null;
				switch (orderTag) {
				    case "ChopWood":
				    case "MineGold":
				    case "MineStone":
				    case "Forage":
					    newOrder = new HarvestFromReservoirOrder(a, target);
					    break;
                    case "ChuckWood":
                        newOrder = new TransmuteOrder(a, target, "wood", "gold");
                        break;
                    case "Meditate":
                        newOrder = new MeditateOrder(a, target);
                        break;
				    case "Hunt":
					    newOrder = new HuntOrder(a, target.GetComponentInParent<Herd>());
					    break;
                    case "Fish":
                        newOrder = new FishOrder(a, target);
                        break;
                    case "Construct":
                        newOrder = new ConstructOrder(a, target);
                        break;
                    case "TearDown":
                        newOrder = new TearDownOrder(a, target);
                        break;
                }

				if (newOrder != null) {
					if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
						a.EnqueueOrder(newOrder);
					} else {
						a.OverrideOrder(newOrder);
					}
				}
			}
		}
	}

	public void IssueMoveOrder(PointerEventData eventData) {
		guiController.HideContextMenu();

		//this should be done diffently, but for some reason I'm getting 0,0,0 world position
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(Camera.main.transform.position, ray.direction, out hit, Mathf.Infinity, 1<<LayerMask.NameToLayer("Terrain"))) {
			foreach (NeolithicObject s in selected) {
				ActorController a = s.GetComponent<ActorController>();
				if (a) {
					BaseOrder move = new SimpleMoveOrder(a, hit.point);
					if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
						a.EnqueueOrder(move);
					} else {
						a.OverrideOrder(move);
					}
				}
			}
		}
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.F5)) {
			string[] ab = getSelectedAbilities();
			if (ab.Length > 0) {
				Debug.Log(string.Join(", ", getSelectedAbilities()));
			} else {
				Debug.Log("No abilities");
			}

		}

		if (boxActive) {
			UpdateBoxSelect();
		}
		if (Input.GetMouseButtonDown (0)) {
			if (!EventSystem.current.IsPointerOverGameObject()) {
				StartBoxSelect();
			}
		}
	}

	public void DeselectAll() {
		guiController.HideSelectionMenu();
		foreach (NeolithicObject s in selected) {
            if (s != null) {
                s.Deselect();
            }
		}
		selected.Clear();
	}

	void OnGUI() {
        //drow the outline for box selection
		if (boxActive) {
			Vector2 start = boxStart;
			Vector2 end = boxEnd;
			
			if ( end.x < start.x) {
				float temp = end.x;
				end = new Vector2(start.x, end.y);
				start = new Vector2(temp, start.y);
			}
			if ( end.y > start.y ) {
				float temp = end.y;
				end = new Vector2(end.x, start.y);
				start = new Vector2(start.x, temp);
			}
			
			GUI.Box (new Rect (start.x, Screen.height-start.y, end.x-start.x, start.y-end.y), "");
		}
	}

    public ResourceReservation ReserveWarehouseResources(ActorController a, string tag, float amount) {
        Warehouse[] warehouses = FindObjectsOfType<Warehouse>();
        foreach (Warehouse w in warehouses) {
            if (w.ReserveContents(a.gameObject, tag, amount)) {
                return a.resourceReservation;
            }
        }
        return null;
    }

    public StorageReservation ReserveStorage(ActorController a, string tag, float amount) {
        Warehouse[] warehouses = FindObjectsOfType<Warehouse>();
        foreach (Warehouse w in warehouses) {
            if (w.ReserveStorage(a.gameObject, tag, amount)) {
                return a.GetComponent<StorageReservation>();
            }
        }
        return null;
    }

    public GameObject CreateResourcePile(string typeTag, float amount) {
        foreach (Resource g in resourcePrefabs) {
            if (g.typeTag == typeTag) { 
                GameObject pile = (GameObject)Instantiate(g.gameObject);
                Resource r = pile.GetComponent<Resource>();
                r.amount = amount;
                return pile;
            }
        }
        throw new ArgumentException("Unable to location prefab for resource tag " + typeTag);
        //return null;
    }

    public void QuickSave() {
        GetComponent<SaverLoader>().SaveGame();
    }

    public void QuickLoad() {
        GetComponent<SaverLoader>().LoadGame();
    }
}
