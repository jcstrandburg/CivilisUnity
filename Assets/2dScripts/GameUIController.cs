using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Reflection;

[assembly:AssemblyVersion("0.1.1.*")]
public class GameUIController : MonoBehaviour {

	GameObject contextMenu;
	InputManager inputManager;
	public SelectionMenuController selectionMenu;
    public SubMenuController subMenu;

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

    // Use this for initialization
    void Start () {
		contextMenu = transform.Find("ContextMenu").gameObject;
		contextMenu.SetActive(false);
		selectionMenu.Hide ();

        Text t = transform.Find("VersionLabel").GetComponent<Text>();
        t.text = string.Format("v{0}", Assembly.GetExecutingAssembly().GetName().Version.ToString());
	}
	
	// Update is called once per frame
	void Update () {	
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
		print ("Executing action: "+action);
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
        gc.RandomizeTerrain();

        NeolithicObject[] objects = GameObject.FindObjectsOfType<NeolithicObject>();
        foreach (NeolithicObject obj in objects) {
            obj.SnapToGround();
            //obj.
        }
    }

    public void ShowBuildMenu() {
        subMenu.ClearMenu();
        subMenu.AddButton("Gold", ()=>GameController.instance.StartBuildingPlacement("Buildings/GoldRocks"));
        subMenu.AddButton("Hut", () => GameController.instance.StartBuildingPlacement("Buildings/Hut"));
        subMenu.AddButton("Stone", () => GameController.instance.StartBuildingPlacement("Buildings/StoneRocks"));
        subMenu.AddButton("Wood", () => GameController.instance.StartBuildingPlacement("Buildings/WoodSource"));
    }
}
