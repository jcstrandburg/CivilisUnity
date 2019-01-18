using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Neolithica.MonoBehaviours;
using Neolithica.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Neolithica.UI {
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
        [Inject]
        public GroundController GroundController { get; set; }

        
        // ReSharper disable once UnusedMember.Local (magic Unity method)
        private void Start() {
            contextMenu = transform.Find("ContextMenu").gameObject;
            contextMenu.SetActive(false);
            selectionMenu.Hide();

            var t = transform.Find("VersionLabel").GetComponent<Text>();
            t.text = $"v{Assembly.GetExecutingAssembly().GetName().Version}";

            MakeDataBindings();
        }

        // ReSharper disable once UnusedMember.Local (magic Unity method)
        private void Update() {
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

        // ReSharper disable once UnusedMember.Local (magic Unity method)
        private void OnGUI() {
            if (paused) {
                GUI.Label(new Rect(200, 200, 200, 200), "paused");
            }
        }

        /// <summary>
        /// Exports databindings
        /// </summary>
        private void MakeDataBindings() {
            //var dbs = GetComponent<DataBindingSource>();

            //dbs.AddBinding("spirit",
            //    () => GameController.Spirit,
            //    (object val) => GameController.Spirit = Convert.ToSingle(val));
            //dbs.AddBinding("dayfactor",
            //    () => GameController.DayCycleController.daytime,
            //    (object val) => GameController.DayCycleController.daytime = Convert.ToSingle(val));
        }

        /// <summary>
        /// Creates a context menu with the given opions and target object
        /// </summary>
        /// <param name="options"></param>
        /// <param name="target"></param>
        [Obsolete]
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
                GameObject contextButton = GameController.Factory.Instantiate(prefab);
                var button = contextButton.GetComponent<Button>();
                CommandType command = option;

                button.onClick.AddListener( () => ExecuteContextAction(command, target.gameObject));
                contextButton.GetComponent<Text>().text = option.ToString() ;
                contextButton.transform.SetParent (contextMenu.transform);
            }
            contextMenu.transform.position = Input.mousePosition;
        }

        /// <summary>
        /// Creates a context menu with the given opions and target object
        /// </summary>
        /// <param name="options">The commands to list in the context menu</param>
        /// <param name="targets">The targets to whome orders should be issued on click.</param>
        /// <param name="issueOrder">Callback that issues an order to a single target.</param>
        /// TODO: Delete ShowContextMenu and rename this
        public void ShowContextMenu2D(IReadOnlyList<CommandType> options, Action<CommandType> issueCommand) {
            if (!options.Any()) {
                HideContextMenu();
                return;
            }

            contextMenu.SetActive(true);
            foreach (Transform child in contextMenu.transform)
                Destroy(child.gameObject);

            var prefab = Resources.Load("ContextTextButton") as GameObject;
            foreach (CommandType option in options) {
                GameObject contextButton = GameController.Factory.Instantiate(prefab);
                var button = contextButton.GetComponent<Button>();
                CommandType command = option;

                button.onClick.AddListener(() => {
                    issueCommand(command);
                    HideContextMenu();
                });
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
        [Obsolete]
        public void ShowSelectionMenu(NeolithicObject selected) {
            selectionMenu.ShowPrimative(selected);
        }

        /// <summary>
        /// Shows a selection menu for the given selected NeolithicObject
        /// </summary>
        /// <param name="selected"></param>
        public void ShowSelectionMenu(Interactible selected) {
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
        public void ExecuteContextAction(CommandType action, GameObject target) {
            GameController.IssueOrder(action, target);
            HideContextMenu();
        }

        /// <summary>
        /// Regenerates the map based on the seed provided in the text box
        /// </summary>
        public void RegenerateMap() {
            var seedText = GameObject.Find("SeedFieldText").GetComponent<Text>();

            float f;
            if (!float.TryParse(seedText.text, out f)) {
                f = UnityEngine.Random.value * 1000.0f;
                seedText.text = Convert.ToString(f, CultureInfo.InvariantCulture);
            }

            GroundController.newGameSettings.seed = f;
            GroundController.GenerateMap();
            GroundController.GenerateResourcesAndDoodads();

            var objects = FindObjectsOfType<NeolithicObject>();
            foreach (NeolithicObject obj in objects) {
                obj.SnapToGround();
            }
        }

        /// <summary>
        /// Finds available techs and shows the technology menu
        /// </summary>
        public void ShowResearchMenu() {
            subMenu.ClearMenu();
            foreach (Technology tech in GameController.GetAvailableTechs()) {
                Button b = subMenu.AddButton(tech.displayName, () => GameController.BuyTech(tech));
                b.interactable = GameController.Spirit >= tech.cost;
            }
        }

        /// <summary>
        /// Finds buildable buildings and displays the build menu
        /// </summary>
        public void ShowBuildMenu() {
            var buildings = GameController.GetBuildableBuildings();
            subMenu.ClearMenu();
            foreach (GameObject building in buildings) {
                GameObject prefab = building;
                subMenu.AddButton(building.name,
                    () => GameController.StartBuildingPlacement(prefab));
            }
        }
    }
}
