using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tofu.Serialization {
    public class SaveGamePacker {

        public static SaveGame PackSaveGame(ICollection<GameObject> saveObjects, IEnumerable<Type> savableMonobehaviourTypes) {
            HashSet<Type> types = new HashSet<Type>(savableMonobehaviourTypes);

            List<MonoBehaviour> behaviours = saveObjects
                .SelectMany(go => go.GetComponents<MonoBehaviour>())
                .Where(mb => types.Contains(mb.GetType()))
                .ToList();

            return new SaveGame { Behaviours = behaviours, GameObjects = saveObjects.ToList() };
        }

        public static SaveGame PackSaveGame(IEnumerable<Type> savableMonobehaviourTypes) {
            List<GameObject> savableObjects = Object.FindObjectsOfType<Savable>()
                .Select(savable => savable.gameObject)
                .ToList();

            return PackSaveGame(savableObjects, savableMonobehaviourTypes);
        }
    }
}