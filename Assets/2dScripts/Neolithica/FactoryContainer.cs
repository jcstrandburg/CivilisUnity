using System.Linq;
using Neolithica.DependencyInjection;
using Neolithica.Extensions;
using UnityEngine;

namespace Neolithica {
    public class FactoryContainer : MonoBehaviour {
        public GameFactoryBase Factory => this.CacheComponent(ref factoryInstance, () => new MainGameFactory(gameObject));
        private GameFactoryBase factoryInstance = null;

        // Handles Awake event
        public void Awake() {
            var gameObjects = FindObjectsOfType<GameObject>().Where(x => x.activeInHierarchy && x.transform.parent == null).ToList();
            Factory.InjectGameObjects(gameObjects);
        }
    }
}