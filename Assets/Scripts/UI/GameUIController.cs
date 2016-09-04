using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System;
using System.Globalization;

[assembly: AssemblyVersion("0.2.2.*")]

public class GameUIController : MonoBehaviour {

	GameObject contextMenu;
	public SelectionMenuController selectionMenu;
    public SubMenuController subMenu;
    public GameObject debugMenu;
    public bool paused = false;

    private GameController _gameController;
    public GameController gameController {
        get {
            if (_gameController == null) {
                _gameController = GameController.Instance;
            }
            return _gameController;
        }
        set { _gameController = value; }
    }

    private void MakeDataBindings() {
        var dbs = GetComponent<DataBindingSource>();

        //dbs.AddBinding("foodbuffer",
        //    () => gameController.foodbuffer,
        //    (object val) => gameController.foodbuffer = Convert.ToSingle(val));
        dbs.AddBinding("spirit",
            () => gameController.spirit,
            (object val) => gameController.spirit = Convert.ToSingle(val));
        dbs.AddBinding("dayfactor",
            () => gameController.daytime,
            (object val) => gameController.daytime = Convert.ToSingle(val));
    }

    // Use this for initialization
    void Start () {
		contextMenu = transform.Find("ContextMenu").gameObject;
		contextMenu.SetActive(false);
		selectionMenu.Hide ();
        
        Text t = transform.Find("VersionLabel").GetComponent<Text>();
        t.text = string.Format("v{0}", Assembly.GetExecutingAssembly().GetName().Version.ToString());

        MakeDataBindings();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Pause)) {
            paused = !paused;
            Time.timeScale = paused ? 0.0f : 1.0f;
        }        
        if (Input.GetKeyDown(KeyCode.BackQuote)) {
            Debug.Log("DEBUG");
            if (debugMenu) {
                debugMenu.SetActive(!debugMenu.activeSelf);
            }
        }
	}

    void OnGUI() {
        if (paused) {
            GUI.Label(new Rect(200, 200, 200, 200), "paused");
        }
    }

	public void ShowContextMenu(string[] options, NeolithicObject target) {
		contextMenu.SetActive(true);
		foreach (Transform child in contextMenu.transform) {
			Destroy(child.gameObject);
		}

		foreach (string o in options) {
            var prefab = Resources.Load("ContextTextButton") as GameObject;
            var temp = gameController.factory.Instantiate(prefab);
			Button button = temp.GetComponent<Button>();
			string capture = o;
			button.onClick.AddListener( () => ExecuteContextAction(capture, target));
			temp.GetComponent<Text>().text = o;
			temp.transform.SetParent (contextMenu.transform);
		}
		contextMenu.transform.position = Input.mousePosition;
	}

	public void HideContextMenu() {
		contextMenu.SetActive (false);
	}

	public void ShowSelectionMenu(NeolithicObject selected) {
		selectionMenu.ShowPrimative(selected);
	}

	public void HideSelectionMenu() {
		selectionMenu.Hide();
	}

	/// <summary>
	/// Forwards a command to the game controller to issue the given order to the current 
	/// selected units against the target, and then hides the context menu
	/// </summary>
	public void ExecuteContextAction(string action, NeolithicObject target) {
        gameController.IssueOrder(action, target);
		HideContextMenu();
	}

    /// <summary>
    /// Regenerates the map based on the seed provided in the text box
    /// </summary>
    public void RegenerateMap() {
        GameObject x = GameObject.Find("SeedFieldText");
        Text t = x.GetComponent<Text>();
        string s = t.text;
        float f = float.Parse(s);

        GameObject y = GameObject.Find("Terrain");
        GroundController gc = y.GetComponent<GroundController>();
        gc.floatSeed = f;
        gc.GenerateMap();

        NeolithicObject[] objects = GameObject.FindObjectsOfType<NeolithicObject>();
        foreach (NeolithicObject obj in objects) {
            obj.SnapToGround();
            //obj.
        }
    }

    public void ShowResearchMenu() {
        Technology[] techs = gameController.GetAvailableTechs();
        subMenu.ClearMenu();
        foreach (Technology t in techs) {
            Technology tech = t;
            Button b = subMenu.AddButton(t.displayName, () => gameController.BuyTech(tech));
			b.interactable = (gameController.spirit >= t.cost);
        }
    }

    public void ShowBuildMenu() {
        var buildings = gameController.GetBuildableBuildings();
        subMenu.ClearMenu();
        foreach (var building in buildings) {
            GameObject prefab = building;
            subMenu.AddButton(building.name,
                            () => gameController.StartBuildingPlacement(prefab));
        }
    }
}
