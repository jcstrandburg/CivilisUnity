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
    /// <summary>A list containing all selected NeolithicObjects</summary>
	public List<NeolithicObject> selected = new List<NeolithicObject>();

    /// <summary>Marquee select start</summary>
    Vector2 boxStart;
    /// <summary>Marquee select end</summary>
	Vector2 boxEnd;
    /// <summary>Is marquee select active</summary>
    public bool boxActive = false;
    /// <summary>Should current selection remain when new objects are selected</summary>
    public bool additiveSelect = false;
    /// <summary>List of resource pile prefabs</summary>
    public List<Resource> resourcePrefabs;
    /// <summary>Manages technology tree</summary>
    public TechManager techManager;
    /// <summary>Manages creation of objects, dependency injection, etc</summary>
    public GameFactory factory = new GameFactory();
    /// <summary>Actions that no actor can currently take</summary>
    public List<string> forbiddenActions;

    private static GameController _instance = null;
    /// <summary>"Singleton" instance getter. Only one of these objects is expected to exists in any scene.</summary>
    public static GameController Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<GameController>();
            }
            return _instance;
        }
    }

    private GameUIController guiController;
    /// <summary>Manages the GuiController object. If no other controller is provided one will be found in the scene.</summary>
    public GameUIController GuiController {
        get {
            if (guiController == null) {
                guiController = FindObjectOfType<GameUIController>();
            }
            return guiController;
        }
        set {
            guiController = value;
        }
    }

    private GroundController groundController;
    /// <summary>Manages the GroundController object. If no other controller is provided one will be found in the scene.</summary>
    public GroundController GroundController {
        get {
            if (groundController == null) {
                groundController = FindObjectOfType<GroundController>();
            }
            return groundController;
        }
        set {
            groundController = value;
        }
    }

    private StatManager statManager;
    /// <summary>Manages the StatManager object. If no other manager is provided one will be found in the scene.</summary>
    public StatManager StatManager {
        get {
            if (statManager == null) {
                statManager = FindObjectOfType<StatManager>();
            }
            return statManager;
        }
        set {
            statManager = value;
        }
    }

    private SaverLoader saverLoader;
    /// <summary>Manages the SaverLoader object. If no other SaverLoader is provided one will be found in the scene.</summary>
    public SaverLoader SaverLoader {
        get {
            if (saverLoader == null) {
                saverLoader = FindObjectOfType<SaverLoader>();
            }
            return saverLoader;
        }
        set {
            saverLoader = value;
        }
    }

    private MenuManager menuManager;
    /// <summary>Manages the MenuManager object. If no other manager is provided one will be found in the scene.</summary>
    public MenuManager MenuManager {
        get {
            if (menuManager == null) {
                menuManager = FindObjectOfType<MenuManager>();
            }
            return menuManager;
        }
        set {
            menuManager = value;
        }
    }

    private LogisticsManager logisticsManager;
    /// <summary>Manages the LogisticsManager object. If no other manager is provided one will be found in the scene.</summary>
    public LogisticsManager LogisticsManager {
        get {
            if (logisticsManager == null) {
                logisticsManager = FindObjectOfType<LogisticsManager>();
            }
            return logisticsManager;
        }
        set {
            logisticsManager = value;
        }
    }

    private DayCycleController dayCycleController;
    /// <summary>Manages the DayCycleController object. If no other controller is provided one will be found in the scene.</summary>
    public DayCycleController DayCycleController {
        get {
            if (dayCycleController == null) {
                dayCycleController = FindObjectOfType<DayCycleController>();
            }
            return dayCycleController;
        }
        set {
            dayCycleController = value;
        }
    }

    public float Spirit { get; set; }

    public BuildingBlueprint buildingPlacer;
    /// <summary>Manages the BuildingPlueprint object. If no other placer is provided one will be found in the scene.</summary>
    private BuildingBlueprint BuildingPlacer {
        get {
            if (buildingPlacer == null) {
                BuildingBlueprint[] bbps = FindObjectsOfType<BuildingBlueprint>();
                if (bbps.Length > 0) {
                    buildingPlacer = bbps[0];
                }
            }
            return buildingPlacer;
        }
    }

    // Handles Awake event
    void Awake() {
        var techs = Resources.LoadAll("Techs", typeof(Technology)).Select(t => (Technology)t).ToArray();
        techManager = new TechManager();
        techManager.LoadArray(techs);

        resourcePrefabs = new List<Resource>();
        var allFiles = Resources.LoadAll<UnityEngine.Object>("");
        resourcePrefabs = allFiles
            .Where(obj => (obj is GameObject && ((GameObject)obj).GetComponent<Resource>() != null))
            .Select(obj => ((GameObject)obj).GetComponent<Resource>())
            .ToList<Resource>();

        InitializeAllObjects();
    }

    // Handles Start event
    void Start () {
    }

    public void InitializeAllObjects() {
        var monoBehaviors = FindObjectsOfType<MonoBehaviour>();
        foreach (var b in monoBehaviors) {
            factory.InjectObject(b);
        }
    }

    // Handles FixedUpdate event
    void FixedUpdate() {
        //remove destroyed objects from selection list
        selected.RemoveAll((s) => (s == null));
        additiveSelect = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
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
		if (t.cost <= this.Spirit) {
			this.Spirit -= t.cost;
			techManager.Research(t);
		}
        GuiController.subMenu.ClearMenu();
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
        MenuManager.PushMenuName("PauseMenu");
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
		GuiController.HideContextMenu();
		if (!selected.Contains(sel)) {
			selected.Add(sel);
		}
		if (selected.Count == 1) {
			GuiController.ShowSelectionMenu(sel);
		} else {
			GuiController.HideSelectionMenu();
		}
	}

    /// <summary>
    /// Starts building placement for the given prefab object
    /// </summary>
    /// <param name="prefab"></param>
    public void StartBuildingPlacement(GameObject prefab) {
        GuiController.subMenu.ClearMenu();
        BuildingPlacer.Activate(prefab);
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
                where cmgr.RequirementsMet(StatManager, techManager)
                select cmgr.gameObject)
                .ToArray();
    }

    /// <summary>
    /// Updates menus and starts a marquee select operation
    /// </summary>
	public void StartBoxSelect() {
		GuiController.HideContextMenu();
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
        var forbidden = new HashSet<string>(forbiddenActions);

		string[] selectedActions = getSelectedAbilities();
		string[] availableActions = selectedActions
            .Intersect(clickee.actionProfile.targetActions)
            .Where((a) => !forbidden.Contains(a))
            .ToArray();
		GuiController.ShowContextMenu(availableActions, clickee);
	}

	/// <summary>
    /// Constructs an order from the orderTag against the given target, 
    /// and assigns it to all selected actors
    /// </summary>
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
                    case "ForestGarden":
                        var prefab = (GameObject)Resources.Load("Buildings/ForestGarden");
                        if (prefab == null) {
                            throw new InvalidOperationException("Can't find prefab");
                        }
                        newOrder = factory.InjectObject(
                           new UpgradeReservoirOrder(a, target, prefab)
                        );
                        break;
                    default:
                        throw new InvalidOperationException("Unrecognized order tag " + orderTag);
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
		GuiController.HideContextMenu();

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
		GuiController.HideSelectionMenu();
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
        var manager = la.logisticsManager;
        var network = manager.FindNearestNetwork(a.transform.position);
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
    public Resource CreateResourcePile(string typeTag, double amount) {
        foreach (Resource g in resourcePrefabs) {
            if (g.typeTag == typeTag) {
                GameObject pile = factory.Instantiate(g.gameObject);
                Resource r = pile.GetComponent<Resource>();
                r.amount = amount;
                return r;
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
