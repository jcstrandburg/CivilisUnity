using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Linq;

[assembly: AssemblyVersion("0.2.3.*")]

/// <summary>
/// Manages game UI functionality
/// </summary>
public class GameUIController : MonoBehaviour {

	GameObject contextMenu;
	public SelectionMenuController selectionMenu;
    public SubMenuController subMenu;
    public GameObject debugMenu;
    public bool paused = false;

    [Inject]
    public GameController GameController { get; set; }

    // Handles Start event
    void Start() {
        contextMenu = transform.Find("ContextMenu").gameObject;
        contextMenu.SetActive(false);
        selectionMenu.Hide();

        Text t = transform.Find("VersionLabel").GetComponent<Text>();
        t.text = string.Format("v{0}", Assembly.GetExecutingAssembly().GetName().Version.ToString());

        MakeDataBindings();
    }

    // Handles Update event
    void Update() {
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

    // Handles OnGUI Event
    void OnGUI() {
        if (paused) {
            GUI.Label(new Rect(200, 200, 200, 200), "paused");
        }
    }

    /// <summary>
    /// Exports databindings
    /// </summary>
    private void MakeDataBindings() {
        var dbs = GetComponent<DataBindingSource>();

        dbs.AddBinding("spirit",
            () => GameController.Spirit,
            (object val) => GameController.Spirit = Convert.ToSingle(val));
        dbs.AddBinding("dayfactor",
            () => GameController.DayCycleController.daytime,
            (object val) => GameController.DayCycleController.daytime = Convert.ToSingle(val));
    }

    /// <summary>
    /// Creates a context menu with the given opions and target object
    /// </summary>
    /// <param name="options"></param>
    /// <param name="target"></param>
	public void ShowContextMenu(CommandType[] options, NeolithicObject target) {
        if (!options.Any()) {
            contextMenu.SetActive(false);
            return;
        }

		contextMenu.SetActive(true);
		foreach (Transform child in contextMenu.transform) {
			Destroy(child.gameObject);
		}

        var prefab = Resources.Load("ContextTextButton") as GameObject;
        foreach (CommandType option in options) {
            var contextButton = GameController.Factory.Instantiate(prefab);
			var button = contextButton.GetComponent<Button>();
            var command = option;
			button.onClick.AddListener( () => ExecuteContextAction(command, target));
			contextButton.GetComponent<Text>().text = option.ToString() ;
			contextButton.transform.SetParent (contextMenu.transform);
		}
		contextMenu.transform.position = Input.mousePosition;
	}

    /// <summary>
    /// Hides the context menu
    /// </summary>
	public void HideContextMenu() {
		contextMenu.SetActive (false);
	}

    /// <summary>
    /// Shows a selection menu for the given selected NeolithicObject
    /// </summary>
    /// <param name="selected"></param>
	public void ShowSelectionMenu(NeolithicObject selected) {
		selectionMenu.ShowPrimative(selected);
	}

    /// <summary>
    /// Hides the selection menu
    /// </summary>
	public void HideSelectionMenu() {
		selectionMenu.Hide();
	}

	/// <summary>
	/// Forwards a command to the game controller to issue the given order to the current 
	/// selected units against the target, and then hides the context menu
	/// </summary>
	public void ExecuteContextAction(CommandType action, NeolithicObject target) {
        GameController.IssueOrder(action, target);
		HideContextMenu();
	}

    /// <summary>
    /// Regenerates the map based on the seed provided in the text box
    /// </summary>
    public void RegenerateMap() {
        GameObject x = GameObject.Find("SeedFieldText");
        Text t = x.GetComponent<Text>();
        string s = t.text;

        float f;
        try {
            f = float.Parse(s);
        } catch {
            f = UnityEngine.Random.value * 1000.0f;
            string s2 = Convert.ToString(f);
            t.text = s2;
            Debug.Log(s2);
        }

        GameObject y = GameObject.Find("Terrain");
        GroundController gc = y.GetComponent<GroundController>();
        gc.floatSeed = f;
        gc.GenerateMap();

        NeolithicObject[] objects = GameObject.FindObjectsOfType<NeolithicObject>();
        foreach (NeolithicObject obj in objects) {
            obj.SnapToGround();
        }
    }

    /// <summary>
    /// Finds available techs and shows the technology menu
    /// </summary>
    public void ShowResearchMenu() {
        Technology[] techs = GameController.GetAvailableTechs();
        subMenu.ClearMenu();
        foreach (Technology t in techs) {
            Technology tech = t;
            Button b = subMenu.AddButton(t.displayName, () => GameController.BuyTech(tech));
			b.interactable = (GameController.Spirit >= t.cost);
        }
    }

    /// <summary>
    /// Finds buildable buildings and displays the build menu
    /// </summary>
    public void ShowBuildMenu() {
        var buildings = GameController.GetBuildableBuildings();
        subMenu.ClearMenu();
        foreach (var building in buildings) {
            GameObject prefab = building;
            subMenu.AddButton(building.name,
                            () => GameController.StartBuildingPlacement(prefab));
        }
    }
}
