using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class LogisticsNetwork : MonoBehaviour {

    GameObject dumpingGround;
    private List<LogisticsNode> nodes = new List<LogisticsNode>();
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
        if (strategy==1) {
            return GetComponentsInChildren<T>();
        } else if (strategy==2) {
            List<T> components = new List<T>();

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
            Debug.Log(">Jobify");
            logisticsManager.UnregisterNetwork(this);
            logisticsManager.RebuildNetworks();
            Debug.Log("<Jobify");
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
            var tags = new List<string> { "meat", "vegetables", "fish" };
            var tagsToRemove = new List<string>();
            var resources = new List<ResourceProfile>();

            foreach (var w in warehouses) {
                foreach (var t in tags) {
                    if (w.GetAvailableContents(t) >= 1) {
                        tagsToRemove.Add(t);
                        w.WithdrawContents(t, 1);
                        resources.Add(new ResourceProfile(t, 1));

                        gameController.MakeToast(w.transform.position, "Food Consumed");
                    }
                }
                foreach (var t in tagsToRemove) {
                    tags.Remove(t);
                }
                tagsToRemove.Clear();

                if (tags.Count == 0) {
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
        var p = resources.OrderBy((ResourceProfile rp) => -rp.amount).ToArray();
        if (p.Count() == 0 || p.Count() > 3) {
            throw new ArgumentException("Unexpected resource count " + resources.Count());
        }

        double returnMe = 0;
        for (int i = 0; i < p.Length; i++) {
            returnMe += (i + 1) * p[i].amount;
        }
        return returnMe;
    }
}
