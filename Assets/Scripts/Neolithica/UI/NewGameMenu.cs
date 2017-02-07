using System;
using Neolithica.MonoBehaviours;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Neolithica.UI {
    public class NewGameMenu : MonoBehaviour {
        NewGameSettings settings = new NewGameSettings();

        void Awake () {
            var dbs = GetComponent<DataBindingSource>();

            settings.seed = UnityEngine.Random.Range(0.0f, 100.0f);
            dbs.AddBinding("seed",
                () => settings.seed,
                (object val) => settings.seed = Convert.ToSingle(val));
            dbs.AddBinding("treeMultiplier",
                () => settings.treeMultiplier,
                (object val) => settings.treeMultiplier = Convert.ToSingle(val));
            dbs.AddBinding("berryMultiplier",
                () => settings.berryMultiplier,
                (object val) => settings.berryMultiplier = Convert.ToSingle(val));
            dbs.AddBinding("stoneRate",
                () => settings.stoneRate,
                (object val) => settings.stoneRate = Convert.ToSingle(val));
            dbs.AddBinding("fishRate",
                () => settings.fishRate,
                (object val) => settings.fishRate = Convert.ToSingle(val));
            dbs.AddBinding("doodadRate",
                () => settings.doodadRate,
                (object val) => settings.doodadRate = Convert.ToSingle(val));
        }

        public void StartNewGame() {
            GameObject go = new GameObject();
            var transition = go.AddComponent<GameSceneTransitioner>();
            transition.InitNewGame(settings);
            SceneManager.LoadScene("PlayGame");
        }
    }
}
