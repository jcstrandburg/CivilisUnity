using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using System;

public class GameController : MonoBehaviour {
	public List<NeolithicObject> selected = new List<NeolithicObject>();
	public GameObject mainLight;
	public GameUIController guiController;
	Vector2 boxStart;
	Vector2 boxEnd;
	public bool boxActive = false;
	public bool additiveSelect = false;
    public BuildingBlueprint buildingPlacer;
    public List<Resource> resourcePrefabs;
    public TechManager techmanager;

    private float _foodbuffer = 10.0f;
    private float _spirit = 0.0f;

    private static GameController _instance = null;

	public static GameController instance
	{
		get { 
			if (_instance == null) {
				GameObject obj = GameObject.Find("_Scripts");
				_instance = obj.GetComponent<GameController>();
			}
			return _instance;
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

	// Use this for initialization
	void Start () {
        UnityEngine.Object[] objects = Resources.LoadAll("Techs", typeof(TextAsset));
        string[] jsonText = Array.ConvertAll(objects, (x) => ((TextAsset)x).text);
        techmanager = new TechManager();
        techmanager.LoadTree(jsonText);
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
        float y = Terrain.activeTerrain.transform.position.y + Terrain.activeTerrain.SampleHeight(pos);
        Vector3 returnMe = pos;
        returnMe.y = y;
        return returnMe;
    }

    /// <summary>
    /// This function appears to be broken atm, don't use until it gets debugged more betterer
    /// </summary>
    /// <param name="pos">The world position we want to get the normal for. This point is not necessarily on the ground, only the x and z are used</param>
    /// <returns>The ground normal vector below the given position</returns>
    public Vector3 GetGroundNormal(Vector3 pos) {
        Terrain t = Terrain.activeTerrain;
        TerrainData td = t.terrainData;
        //Debug.Log(pos);
        Vector3 terrainLocal = pos - t.transform.position;
        //Debug.Log(terrainLocal);
        Vector2 normalizedPos = new Vector2(Mathf.InverseLerp(0.0f, td.size.x, terrainLocal.x),
                                            Mathf.InverseLerp(0.0f, td.size.y, terrainLocal.z));
        //Debug.Log(normalizedPos);
        Vector3 normal = td.GetInterpolatedNormal(normalizedPos.x, normalizedPos.y);
        return normal;
    }

	void FixedUpdate() {
		mainLight.transform.eulerAngles += new Vector3(0, 0.2f, 0);
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

    public void StartBuildingPlacement(string type) {
        GameUIController.instance.subMenu.ClearMenu();
        buildingPlacer.Activate(type);
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
					    newOrder = new TempHarvestOrder(a, target);
					    break;
                    case "ChuckWood":
                        newOrder = new TransmuteOrder(a, target, "wood", "gold");
                        break;
                    case "Meditate":
                        newOrder = new MeditateOrder(a, target);
                        break;
				    case "Hunt":
					    newOrder = new TempHuntOrder(a, target.GetComponentInParent<Herd>());
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
			s.Deselect();
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

    public GameObject CreateResourcePile(string typeTag) {
        foreach (Resource g in resourcePrefabs) {
            if (g.typeTag == typeTag) { 
                GameObject pile = (GameObject)Instantiate(g.gameObject);
                return pile;
            }
        }
        return null;
    }
}
