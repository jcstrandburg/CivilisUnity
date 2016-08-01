using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System;
using System.Globalization;

[assembly: AssemblyVersion("0.2.0.*")]

public class GameUIController : MonoBehaviour {

	GameObject contextMenu;
	public SelectionMenuController selectionMenu;
    public SubMenuController subMenu;
    public GameObject debugMenu;
    public bool paused = false;

    private List<DataBinding> dataBindings = new List<DataBinding>();
    private static GameUIController _instance = null;

    public static GameUIController instance {
        get {
            if (_instance == null) {
                GameObject obj = GameObject.Find("GameUI");
                _instance = obj.GetComponent<GameUIController>();
            }
            return _instance;
        }
    }

    private void MakeDataBindings() {
        dataBindings = new List<DataBinding>();

        Text spiritDataText = GameObject.Find("SpiritData").GetComponent<Text>();
        if (spiritDataText) {
            dataBindings.Add(new OneWayBinding<float>(() => {
                return GameController.instance.spirit;
            }, (f) => {
				spiritDataText.text = f.ToString("N1");
            }));
        }
        Text foodbufferDataText = GameObject.Find("FoodbufferData").GetComponent<Text>();
        if (foodbufferDataText) {
            dataBindings.Add(new OneWayBinding<float>(() => {
                return GameController.instance.foodbuffer;
            }, (f) => {
                foodbufferDataText.text = f.ToString("N1");
            }));
        }
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
        dataBindings.ForEach((db) => {
            db.Update();
        });

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
			GameObject temp = Instantiate (Resources.Load ("ContextTextButton")) as GameObject;
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
		GameController.instance.IssueOrder(action, target);
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
        Technology[] techs = GameController.instance.GetAvailableTechs();
        subMenu.ClearMenu();
        foreach (Technology t in techs) {
            Technology tech = t;
            Button b = subMenu.AddButton(t.displayName, () => GameController.instance.BuyTech(tech));
			b.interactable = (GameController.instance.spirit >= t.cost);
        }
    }

    public void ShowBuildMenu() {
        var buildings = GameController.instance.GetBuildableBuildings();

        subMenu.ClearMenu();
        foreach (var building in buildings) {
            var gc = GameController.instance;
            GameObject prefab = building;
            subMenu.AddButton(building.name,
                            () => gc.StartBuildingPlacement(prefab));
        }
    }
}
