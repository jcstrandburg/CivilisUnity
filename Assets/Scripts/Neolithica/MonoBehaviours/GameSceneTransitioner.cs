using UnityEngine;
using UnityEngine.SceneManagement;

namespace Neolithica.MonoBehaviours {
    /// <summary>
    /// Responds to scene change events by loading a saved game or starting a new game
    /// </summary>
    public class GameSceneTransitioner : MonoBehaviour {

        /// <summary>The settings to apply when a new game is loaded</summary>
        private NewGameSettings newGameSettings;
        /// <summary>Name of the save game file to load when the scene transitions</summary>
        private string loadGameName = null;

        [Inject]
        public GameController GameController { get; set; }
        [Inject]
        public GroundController GroundController { get; set; }

        // Handles Start event
        public void Start() {
            DontDestroyOnLoad(gameObject);
        }

        // Handles OnEnable event
        public void OnEnable() {
            SceneManager.sceneLoaded += DoSceneTransition;
        }

        // Handles OnDisable event
        public void OnDisable() {
            SceneManager.sceneLoaded -= DoSceneTransition;
        }

        /// <summary>
        /// Initializes the transitioner to load a saved game on scene change
        /// </summary>
        /// <param name="gameGame"></param>
        public void InitLoadGame(string gameGame) {
            loadGameName = gameGame;
        }

        /// <summary>
        /// Initializes the transitioner to create a new game on scene change
        /// </summary>
        /// <param name="settings"></param>
        public void InitNewGame(NewGameSettings settings) {
            newGameSettings = settings;
        }

        /// <summary>
        /// Transitions to the gameplay scene, either initiating a new game or a game load as appropriate
        /// </summary>
        /// <param name="scene">The scene to transition to</param>
        /// <param name="mode">Unused, necessary to fit signature of <c>SceneManager.sceneLoaded</c></param>
        private void DoSceneTransition(Scene scene, LoadSceneMode mode) {
            if (loadGameName != null) {
                GameController.Instance.SaverLoader.LoadGame(loadGameName);
            }
            else {
                GroundController.ApplySettings(newGameSettings);
                GroundController.GenerateMap();
                GroundController.GenerateResourcesAndDoodads();
            }
            Destroy(gameObject);
        }
    }
}
