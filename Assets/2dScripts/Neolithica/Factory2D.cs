using System.Collections.Generic;
using System.Linq;
using Neolithica.DependencyInjection;
using Neolithica.Extensions;
using UnityEngine;

namespace Neolithica {
    public class Factory2D : MonoBehaviour {

        /// <summary>Manages creation of objects, dependency injection, etc</summary>
        public FactoryFactory Factory => this.CacheComponent(ref factoryInstance, () => new FactoryFactory(gameObject));
        private FactoryFactory factoryInstance;

        public void Awake () {
            var gameObjects = FindObjectsOfType<GameObject>().Where(x => x.activeInHierarchy && x.transform.parent == null).ToList();
            Factory.InjectGameObjects(gameObjects);
        }

        public class FactoryFactory : GameFactoryBase {
            public FactoryFactory(GameObject rootObject) : base(BuildResolvers(rootObject)) {}

            private static IEnumerable<IDependencyResolver> BuildResolvers(GameObject rootObject) =>
                Enumerable.Empty<IDependencyResolver>();
        }
    }
}
