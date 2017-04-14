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

        private List<ResourceSettings> resourceSettings = new List<ResourceSettings> {
            new ResourceSettings {Type = ResourcePlacementType.Berries, Frequency = 4.0f, Abundance = 0.3f},
            new ResourceSettings {Type = ResourcePlacementType.Trees, Frequency = 1.5f, Abundance = 0.5f},
            new ResourceSettings {Type = ResourcePlacementType.Fish, Frequency = 5.0f, Abundance = 0.2f},
            new ResourceSettings {Type = ResourcePlacementType.Gold, Frequency = 2.0f, Abundance = 0.2f},
            new ResourceSettings {Type = ResourcePlacementType.Stone, Frequency = 2.0f, Abundance = 0.2f},
            new ResourceSettings {Type = ResourcePlacementType.Doodad, Frequency = 10.0f, Abundance = 0.15f},
        };

        void Awake () {
            var dbs = GetComponent<DataBindingSource>();
            var resourceSettingsDict = resourceSettings.ToDictionary(r => r.Type);

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
