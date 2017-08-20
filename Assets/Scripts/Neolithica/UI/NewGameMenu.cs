using System;
using Neolithica.MonoBehaviours;
using Neolithica.TerrainGeneration;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

namespace Neolithica.UI {

    public class NewGameMenu : MonoBehaviour {
        private NewGameSettings settings = new NewGameSettings();

        void Awake () {
            var dbs = GetComponent<DataBindingSource>();
            var resourceSettingsDict = settings.ResourceSettings.ToDictionary(r => r.Type);

            settings.seed = UnityEngine.Random.Range(0.0f, 1000.0f);
            dbs.AddBinding("seed", () => settings.seed, (object val) => settings.seed = Convert.ToSingle(val));

            BindResourceSetting(dbs, "treeFrequency", "treeAbundance", resourceSettingsDict[ResourcePlacementType.Trees]);
            BindResourceSetting(dbs, "berriesFrequency", "berriesAbundance", resourceSettingsDict[ResourcePlacementType.Berries]);
            BindResourceSetting(dbs, "stoneFrequency", "stoneAbundance", resourceSettingsDict[ResourcePlacementType.Stone]);
            BindResourceSetting(dbs, "goldFrequency", "goldAbundance", resourceSettingsDict[ResourcePlacementType.Gold]);
            BindResourceSetting(dbs, "fishFrequency", "fishAbundance", resourceSettingsDict[ResourcePlacementType.Fish]);
            BindResourceSetting(dbs, "doodadsFrequency", "doodadsAbundance", resourceSettingsDict[ResourcePlacementType.Doodad]);
        }

        public void StartNewGame() {
            GameObject go = new GameObject();
            var transition = go.AddComponent<GameSceneTransitioner>();

            transition.InitNewGame(settings);
            SceneManager.LoadScene("PlayGame");
        }

        private void BindResourceSetting(DataBindingSource bindingSource, string frequencyName, string abundanceName, ResourceSettings setting) {
            bindingSource.AddBinding(frequencyName, () => setting.Frequency, val => setting.Frequency = Convert.ToSingle(val));
            bindingSource.AddBinding(abundanceName, () => setting.Abundance, val => setting.Abundance = Convert.ToSingle(val));
        }
    }
}
