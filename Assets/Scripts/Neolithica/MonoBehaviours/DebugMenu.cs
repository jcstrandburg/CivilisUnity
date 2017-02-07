using Neolithica.Serialization;
using UnityEngine;

namespace Neolithica.MonoBehaviours {
    public class DebugMenu : MonoBehaviour {

        [Inject]
        public GameController GameController { get; set; }

        // Use this for initialization
        void Start () {	
        }
	
        // Update is called once per frame
        void Update () {	
        }

        public void QuickSave() {
            GameController.QuickSave();
        }

        public void QuickLoad() {
            GameController.QuickLoad();
        }

        public void AddWorker() {
            GameObject prefab = Resources.Load("Units/Worker") as GameObject;
            GameObject newWorker = GameController.Factory.Instantiate(prefab);
            newWorker.transform.position = Camera.main.transform.position;
        }

        public void GameSpeed() {
            Time.timeScale *= 2;
            if (Time.timeScale > 4.0f) {
                Time.timeScale = 0.25f;
            }
            Debug.Log("Timescale: " + Time.timeScale);
        }

        public void PauseGame() {
            GameController.PauseGame();
        }

        public void ShowSaveGames() {
            string[] games = GameController.GetComponent<SaverLoader>().GetSaveGames();
            Debug.Log("Found " + games.Length + " save games");
            foreach (string s in games) {
                Debug.Log(s);
            }
        }
    }
}
