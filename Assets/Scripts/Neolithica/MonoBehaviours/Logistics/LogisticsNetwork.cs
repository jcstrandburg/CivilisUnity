using System;
using System.Collections.Generic;
using System.Linq;
using Neolithica.Extensions;
using Tofu.Serialization;
using UnityEngine;

namespace Neolithica.MonoBehaviours.Logistics {
    [SavableMonobehaviour(22)]
    public class LogisticsNetwork : MonoBehaviour {

        public int strategy = 1;

        public int NodeCount => nodes.Count;

        [SerializeField]
        private double foodbuffer = 6.0;
        public double Foodbuffer {
            get {
                return foodbuffer;
            }
            set {
                foodbuffer = value;
            }
        }

        private LogisticsManager logisticsManager;
        [Inject]
        public LogisticsManager LogisticsManager {
            set {
                if (logisticsManager && logisticsManager != value) {
                    logisticsManager.UnregisterNetwork(this);
                    logisticsManager = null;
                }
                if (logisticsManager == null && value != null) {
                    logisticsManager = value;
                    logisticsManager.RegisterNetwork(this);
                }
            }
            get {
                return logisticsManager;
            }
        }

        [Inject]
        public GameController GameController { get; set; }

        public void Start() {
            InvokeRepeating(nameof(KeepFoodBufferFilled), 1.0f, 0.5f);
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
            return GetComponentsInChildren<T>();
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
        private void KeepFoodBufferFilled() {
            if (Foodbuffer >= 3.0) return;

            var warehouses = FindComponents<Warehouse>();
            var foodKinds = new List<ResourceKind> { ResourceKind.Meat, ResourceKind.Vegetables, ResourceKind.Fish};
            var typesToRemove = new List<ResourceKind>();
            var resources = new List<ResourceProfile>();

            foreach (Warehouse w in warehouses) {
                foreach (ResourceKind t in foodKinds) {
                    if (!(w.GetAvailableContents(t) >= 1)) continue;
                    typesToRemove.Add(t);
                    w.WithdrawContents(t, 1);
                    resources.Add(new ResourceProfile(t, 1));

                    GameController.MakeToast(w.transform.position, "Food Consumed");
                }
                foreach (var t in typesToRemove) {
                    foodKinds.Remove(t);
                }
                typesToRemove.Clear();

                if (foodKinds.Count == 0) {
                    break;
                }
            }

            if (resources.Count > 0) {
                Foodbuffer += CalcFoodValue(resources);
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
