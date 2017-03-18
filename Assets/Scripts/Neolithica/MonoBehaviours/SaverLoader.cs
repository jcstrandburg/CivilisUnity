using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Tofu.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Neolithica.MonoBehaviours {
    public class SaverLoader : MonoBehaviour {

        public string LoadIntoScene = null;

        public void OnEnable() {
            TypeModelBuilderBase.CacheBaseModel();
        }

        public void SaveGame(string name = "Quick") {
            if (name == null)
                throw new ArgumentNullException("name");

            var path = Application.persistentDataPath + "/Saved Games/" + name + ".sav";
            try {
                ITypeModelBuilder builder = new AttributeBasedTypeModelBuilder();
                var serializer = new GameSerializer(GetPrefabs(), builder);
                SaveGame saveGame = SaveGamePacker.PackSaveGame(GetSavables(), builder.GetSavableMonobehaviours());

                using (var stream = new FileStream(path, FileMode.Create)) {
                    serializer.Serialize(stream, saveGame);
                }
            }
            catch (Exception e) {
                Debug.Log(e);
            }
        }

        public void LoadGame(string name = "Quick") {
            GameObject oldRoot = GameController.Instance.gameObject;
            DestroyImmediate(oldRoot); // this is generally a bad idea, but it's necessary for our dependency injection to work

            if (name == null)
                throw new ArgumentNullException("name");

            var path = Application.persistentDataPath + "/Saved Games/" + name + ".sav";
            if (string.IsNullOrEmpty(LoadIntoScene)) {
                ITypeModelBuilder builder = new AttributeBasedTypeModelBuilder();
                var serializer = new GameSerializer(GetPrefabs(), builder);

                using (var stream = new FileStream(path, FileMode.Open)) {
                    serializer.Deserialize<SaveGame>(stream);
                }

                GameObject newRoot = GameController.Instance.gameObject;
                IEnumerable<GameObject> prefabChildObjectsNotRestored = newRoot
                    .GetComponentsInChildren<Savable>()
                    .Where(savable => !savable.WasRestored)
                    .Select(savable => savable.gameObject);

                foreach (var gameObject in prefabChildObjectsNotRestored) {
                    Destroy(gameObject);
                }

                // It is necessary to call this here even though GameController attempts to do injection because 
                // the whole scene graph may not have been fully instantiated yet when GameController does the injection
                GameController.Instance.InjectAllObjects();
            }
            else {
                GameObject go = new GameObject();
                var transition = go.AddComponent<GameSceneTransitioner>();
                transition.InitLoadGame(name);
                SceneManager.LoadScene(LoadIntoScene);
            }
        }

        public string[] GetSaveGames() {
            string path = Application.persistentDataPath + "/Saved Games/";
            return Directory
                .GetFiles(path)
                .Select<string, string>(Path.GetFileNameWithoutExtension)
                .ToArray();
        }

        private ReadOnlyCollection<GameObject> GetSavables() {
            return GameController.Instance
                .GetComponentsInChildren<Savable>()
                .Select(savable => savable.gameObject)
                .ToReadOnlyCollection();
        }

        private ReadOnlyCollection<GameObject> GetPrefabs() {
            return Resources.LoadAll<GameObject>("")
                .Where(prefab => prefab.GetComponents<Savable>().Any())
                .ToReadOnlyCollection();
        }
    }
}
