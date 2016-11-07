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
    public GameObject moonLight;
    Vector2 boxStart;
	Vector2 boxEnd;
	public bool boxActive = false;
	public bool additiveSelect = false;
    public List<Resource> resourcePrefabs;
    public TechManager techManager;
    public float daytime = 0.5f;
    public float daylength = 10.0f;

    public GameFactory factory = new GameFactory();

    private static GameController _instance = null;
    public static GameController Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<GameController>();
            }
            return _instance;
        }
    }

    private GameUIController _guiController;
    public GameUIController guiController {
        get {
            if (_guiController == null) {
                _guiController = FindObjectOfType<GameUIController>();
            }
            return _guiController;
        }
        set {
            _guiController = value;
        }
    }

    private GroundController _groundController;
    public GroundController groundController {
        get {
            if (_groundController == null) {
                _groundController = FindObjectOfType<GroundController>();
            }
            return _groundController;
        }
        set {
            _groundController = value;
        }
    }

    private StatManager _statManager;
    public StatManager statManager {
        get {
            if (_statManager == null) {
                _statManager = FindObjectOfType<StatManager>();
            }
            return _statManager;
        }
        set {
            _statManager = value;
        }
    }

    private SaverLoader _saverLoader;
    public SaverLoader saverLoader {
        get {
            if (_saverLoader == null) {
                _saverLoader = FindObjectOfType<SaverLoader>();
            }
            return _saverLoader;
        }
        set {
            _saverLoader = value;
        }
    }

    private MenuManager _menuManager;
    public MenuManager menuManager {
        get {
            if (_menuManager == null) {
                _menuManager = FindObjectOfType<MenuManager>();
            }
            return _menuManager;
        }
        set {
            _menuManager = value;
        }
    }

    private LogisticsManager _logisticsManager;
    public LogisticsManager logisticsManager {
        get {
            if (_logisticsManager == null) {
                _logisticsManager = FindObjectOfType<LogisticsManager>();
            }
            return _logisticsManager;
        }
        set {
            _logisticsManager = value;
        }
    }

    public float spirit { get; set; }

    public BuildingBlueprint _buildingPlacer;
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

    // Handles Start event
    void Start () {
        guiController = GameObject.Find("GameUI").GetComponent<GameUIController>();
        var techs = from t in Resources.LoadAll("Techs", typeof(Technology))
                    select (Technology)t;
        techManager = new TechManager();
        techManager.LoadArray(techs.ToArray());

        resourcePrefabs = new List<Resource>();
        var allFiles = Resources.LoadAll<UnityEngine.Object>("");
        foreach (var obj in allFiles) {
            if (obj is GameObject) {
                if (((GameObject)obj).GetComponent<Resource>() != null) {
                    resourcePrefabs.Add(((GameObject)obj).GetComponent<Resource>());
                }
            }
        }        
    }

    // Handles FixedUpdate event
    void FixedUpdate() {
        //remove destroyed objects from selection list
        selected.RemoveAll((s) => (s == null));

        additiveSelect = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        //daytime += Time.fixedDeltaTime / daylength;
        //daytime = daytime % 1.0f;

        float x = Mathf.Sin((float)(daytime * Math.PI));
        RenderSettings.ambientIntensity = Mathf.Lerp(0.8f, 1.0f, x);
        float y = Mathf.Lerp(0.55f, 1.0f, x);
        RenderSettings.ambientLight = new Color(1.0f, y, y);

        float x2 = Mathf.Sin((float)((-0.25f + daytime) * 2 * Math.PI));
        var light = mainLight.GetComponent<Light>();
        light.color = new Color(1.0f, y, y);
        light.intensity = x2 * 1.0f;
        mainLight.transform.eulerAngles = new Vector3((daytime - 0.25f) * 360.0f, 0, 0);

        float x3 = Mathf.Cos((float)((daytime) * Math.PI));
        //Debug.Log(x3);
        var light2 = moonLight.GetComponent<Light>();
        light2.color = new Color(1.0f, 1.0f, 1.0f);
        light2.intensity = x3 * 0.2f;
        moonLight.transform.eulerAngles = new Vector3((daytime + 0.25f) * 360.0f, 0, 0);
    }

    // Handles Update event
    void Update() {
        if (Input.GetKeyDown(KeyCode.F5)) {
            string[] ab = getSelectedAbilities();
            if (ab.Length > 0) {
                Debug.Log(string.Join(", ", getSelectedAbilities()));
            }
            else {
                Debug.Log("No abilities");
            }

        }

        if (boxActive) {
            UpdateBoxSelect();
        }
        if (Input.GetMouseButtonDown(0)) {
            if (!EventSystem.current.IsPointerOverGameObject()) {
                StartBoxSelect();
            }
        }
    }

    // handles OnGUI event
    void OnGUI() {
        //drow the outline for box selection
        if (boxActive) {
            Vector2 start = boxStart;
            Vector2 end = boxEnd;

            if (end.x < start.x) {
                float temp = end.x;
                end = new Vector2(start.x, end.y);
                start = new Vector2(temp, start.y);
            }
            if (end.y > start.y) {
                float temp = end.y;
                end = new Vector2(end.x, start.y);
                start = new Vector2(start.x, temp);
            }

            GUI.Box(new Rect(start.x, Screen.height - start.y, end.x - start.x, start.y - end.y), "");
        }
    }

    // Handles OnDeserialize event
    void OnDeserialize() {
        selected.Clear();
    }

    /// <summary>
    /// Gets all elligible techs from the TechManager
    /// </summary>
    /// <returns></returns>
    public Technology[] GetAvailableTechs() {
        return techManager.GetEligibleTechs();
    }

    /// <summary>
    /// Purchases a technology through the TechManager
    /// </summary>
    /// <param name="t"></param>
    public void BuyTech(Technology t) {
        Debug.Log("Researching tech: " + t.techName);
		if (t.cost <= this.spirit) {
			this.spirit -= t.cost;
			techManager.Research(t);
		}
        guiController.subMenu.ClearMenu();
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

    /// <summary>
    /// Pauses the game simulation
    /// </summary>
    public void PauseGame() {
        Time.timeScale = 0.0f;
        menuManager.PushMenuName("PauseMenu");
    }

    /// <summary>
    /// Unpauses the game simulation
    /// </summary>
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
    /// Attempts to withdraw the given resource profile from any warehouse. 
    /// Note that on failure to withdraw the full amount it is possible that partial widthrawals may 
    /// happen and then those resources will be lost permanently. This function should only be called
    /// when total available resources have already been verified.
    /// </summary>
    /// <param name="rp"></param>
    /// <returns>True on success, false on failure</returns>
    public bool WithdrawFromAnyWarehouse(ResourceProfile rp) {
        var warehouses = FindObjectsOfType<Warehouse>();
        foreach (var w in warehouses) {
            double avail = w.GetAvailableContents(rp.resourceTag);
            double amount = (rp.amount < avail ? rp.amount : avail);
            if (amount > 0) {
                w.WithdrawContents(rp.resourceTag, amount);
                rp.amount -= amount;
            }

            if (rp.amount <= 0) {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Gets all available contents in all warehouses in game total
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, double> GetAllAvailableResources() {
        var d = new Dictionary<string, double>();
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

    /// <summary>
    /// Adds a NeolithicObject to the selected objects and updates the selection menu
    /// </summary>
    /// <param name="sel"></param>
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

    /// <summary>
    /// Starts building placement for the given prefab object
    /// </summary>
    /// <param name="prefab"></param>
    public void StartBuildingPlacement(GameObject prefab) {
        guiController.subMenu.ClearMenu();
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
				sharedAbilities = s.actionProfile.abilities;
			} else {
				sharedAbilities = s.actionProfile.abilities.Intersect(sharedAbilities);
			}
		}
		return sharedAbilities.ToArray();
	}

    /// <summary>
    /// Gets all buildable building from the Buildings resource folder
    /// </summary>
    /// <returns>Buildable building</returns>
    public GameObject[] GetBuildableBuildings() {
        var constructionManagers = from o in Resources.LoadAll("Buildings", typeof(ConstructionManager))
                                   select (ConstructionManager)o;
        return (from cmgr in constructionManagers
                where cmgr.RequirementsMet(statManager, techManager)
                select cmgr.gameObject)
                .ToArray();
    }

    /// <summary>
    /// Updates menus and starts a marquee select operation
    /// </summary>
	public void StartBoxSelect() {
		guiController.HideContextMenu();
		boxEnd = boxStart = Input.mousePosition;
		boxActive = true;
	}
    
    /// <summary>
    /// Updates the marquee select as well as updating NeolithicObjects under the marquee select
    /// </summary>
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

    /// <summary>
    /// Disables the marquee select and selects all of the given NeolithicObjects
    /// </summary>
    /// <param name="selectables"></param>
	public void EndBoxSelect(IEnumerable<NeolithicObject> selectables) {
		boxActive = false;
		if (!additiveSelect) {
			DeselectAll();
		}
		foreach (NeolithicObject no in selectables) {
			no.HoverEnd();
			no.Select();
		}
	}

    /// <summary>
    /// Sets up a context menu for the given clicked object based on the abilities of the selected object(s)
    /// </summary>
    /// <param name="clickee"></param>
	public void DoContextMenu(NeolithicObject clickee) {
		string[] selectedActions = getSelectedAbilities();
		string[] availableActions = selectedActions.Intersect(clickee.actionProfile.targetActions).ToArray();
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

    /// <summary>
    /// Issues a move order to all selected Actor objects
    /// </summary>
    /// <param name="eventData"></param>
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

    /// <summary>
    /// Deselects all currently selected NeolithicObjects
    /// </summary>
    public void DeselectAll() {
		guiController.HideSelectionMenu();
		foreach (NeolithicObject s in selected) {
            if (s != null) {
                s.Deselect();
            }
		}
		selected.Clear();
	}

    /// <summary>
    /// Gets a resource reservation from any available warehouse and attaches it to the given actor
    /// </summary>
    /// <param name="a"></param>
    /// <param name="tag"></param>
    /// <param name="amount"></param>
    /// <returns>The reservation created, or null on failure</returns>
    public ResourceReservation ReserveWarehouseResources(ActorController a, string tag, double amount) {
        var la = a.GetComponent<LogisticsActor>();
        var network = la.logisticsManager.FindNearestNetwork(a.transform.position);

        Warehouse[] warehouses = network.FindComponents<Warehouse>();
        foreach (Warehouse w in warehouses) {
            if (w.ReserveContents(a.gameObject, tag, amount)) {
                return a.resourceReservation;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets a storage reservation and attaches it to the given actor.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="tag"></param>
    /// <param name="amount"></param>
    /// <returns>The reservaton created, or null on failure</returns>
    public StorageReservation ReserveStorage(ActorController a, string tag, double amount) {
        var la = a.GetComponent<LogisticsActor>();
        var network = la.logisticsManager.FindNearestNetwork(a.transform.position);
        if (network != null) {
            Warehouse[] warehouses = network.FindComponents<Warehouse>();
            foreach (Warehouse w in warehouses) {
                if (w.ReserveStorage(a.gameObject, tag, amount)) {
                    return a.GetComponent<StorageReservation>();
                }
            }
        } else {
            Debug.Log("Unable to locate logistics network");
        }
        return null;
    }

    /// <summary>
    /// Creates a new resource pile
    /// </summary>
    /// <param name="typeTag"></param>
    /// <param name="amount"></param>
    /// <returns>A reference to the new pile's GameObject</returns>
    public GameObject CreateResourcePile(string typeTag, double amount) {
        foreach (Resource g in resourcePrefabs) {
            if (g.typeTag == typeTag) {
                GameObject pile = factory.Instantiate(g.gameObject);
                Resource r = pile.GetComponent<Resource>();
                r.amount = amount;
                return pile;
            }
        }
        throw new ArgumentException("Unable to location prefab for resource tag " + typeTag);
    }

    /// <summary>
    /// Saves the game to the default quickload save file
    /// </summary>
    public void QuickSave() {
        GetComponent<SaverLoader>().SaveGame();
    }

    /// <summary>
    /// Loades the game from the default quickload save file
    /// </summary>
    public void QuickLoad() {
        GetComponent<SaverLoader>().LoadGame();
    }

    /// <summary>
    /// Creates a "toast" popup notification
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="message"></param>
    public void MakeToast(Vector3 pos, string message) {
        var prefab = Resources.Load("Toast") as GameObject;
        var toast = factory.Instantiate(prefab);
        toast.GetComponent<Toast>().Init(pos, message);
    }
}
