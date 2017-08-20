using System;
using System.Collections.Generic;
using System.Linq;
using Tofu.Serialization;
using UnityEngine;

namespace Neolithica.MonoBehaviours.Logistics {
    [SavableMonobehaviour(22)]
    public class LogisticsNetwork : MonoBehaviour {

        public int strategy = 1;

        public int NodeCount {
            get {
                return nodes.Count;
            }
        }

        [SerializeField]
        private double _foodbuffer = 6.0;
        public double foodbuffer {
            get {
                return _foodbuffer;
            }
            set {
                _foodbuffer = value;
            }
        }

        private LogisticsManager _manager;
        [Inject]
        public LogisticsManager logisticsManager {
            set {
                if (_manager && _manager != value) {
                    _manager.UnregisterNetwork(this);
                    _manager = null;
                }
                if (_manager == null && value != null) {
                    _manager = value;
                    _manager.RegisterNetwork(this);
                }
            }
            get {
                return _manager;
            }
        }

        private GameController _gameController;
        [Inject]
        public GameController gameController {
            get {
                if (_gameController == null) {
                    _gameController = GameController.Instance;
                }
                return _gameController;
            }
            set { _gameController = value; }
        }

        public void Start() {
            InvokeRepeating("KeepFoodBufferFilled", 1.0f, 0.5f);
        }

        public void AttachNode(LogisticsNode node) {
            if (nodes.Contains(node)) {
                throw new InvalidOperationException("Node already registered");
            }
            nodes.Add(node);
            node.transform.parent = transform;
        }

        public void DetachNode(LogisticsNode node) {
            nodes.Remove(node);
            node.transform.parent = transform.parent;
        }

        public T[] FindComponents<T>() {
            //I don't recall why I did this....
            if (strategy==1) {
                return GetComponentsInChildren<T>();
            } else if (strategy==2) {
                var components = new List<T>();

                components.AddRange(GetComponents<T>());
                foreach (var n in nodes) {
                    components.AddRange(n.GetComponents<T>());
                }
                return components.ToArray();
            } else {
                throw new Exception("Invalid strategy");
            }
        }

        public void OnDestroy() {
            if (logisticsManager != null) {
                logisticsManager.UnregisterNetwork(this);
                logisticsManager.RebuildNetworks();
            }
        }

        /// <summary>
        /// Run in the background via InvokeRepeating, attempts to consume 
        /// food from any source to keep the food buffer filled
        /// </summary>
        /// <todo>Rework to use a logistics system</todo>
        void KeepFoodBufferFilled() {
            if (foodbuffer < 3.0) {
                var warehouses = FindComponents<Warehouse>();
                var types = new List<ResourceKind> { ResourceKind.Meat, ResourceKind.Vegetables, ResourceKind.Fish};
                var typesToRemove = new List<ResourceKind>();
                var resources = new List<ResourceProfile>();

                foreach (var w in warehouses) {
                    foreach (var t in types) {
                        if (!(w.GetAvailableContents(t) >= 1)) continue;
                        typesToRemove.Add(t);
                        w.WithdrawContents(t, 1);
                        resources.Add(new ResourceProfile(t, 1));

                        gameController.MakeToast(w.transform.position, "Food Consumed");
                    }
                    foreach (var t in typesToRemove) {
                        types.Remove(t);
                    }
                    typesToRemove.Clear();

                    if (types.Count == 0) {
                        break;
                    }
                }

                if (resources.Count > 0) {
                    foodbuffer += CalcFoodValue(resources);
                }
            }
        }

        /// <summary>
        /// Calculates the value of a given collection of food resources, 
        /// with increasing value for greater variety of food types
        /// </summary>
        /// <param name="resources"></param>
        /// <returns>Food value</returns>
        public double CalcFoodValue(IEnumerable<ResourceProfile> resources) {
            var p = resources.OrderBy((ResourceProfile rp) => -rp.Amount).ToList();
            if (p.Count == 0 || p.Count() > 3) {
                throw new ArgumentException($"Unexpected resource count {p.Count}", nameof(resources));
            }

            double returnMe = 0;
            for (int i = 0; i < p.Count; i++) {
                returnMe += (i + 1) * p[i].Amount;
            }
            return returnMe;
        }

        private List<LogisticsNode> nodes = new List<LogisticsNode>();
    }
}
