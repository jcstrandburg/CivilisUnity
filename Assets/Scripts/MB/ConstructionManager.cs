using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ConstructionManager))]
public class ConstructionManagerEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        ConstructionManager cm = (ConstructionManager)target;
        if (GUILayout.Button("Ghost Good")) {
            cm.GhostGood();
        }
        if (GUILayout.Button("Ghost Bad")) {
            cm.GhostBad();
        }
        if (GUILayout.Button("Ungost")) {
            cm.UnGhost();
        }
    }
}
#endif

[Serializable]
public class BuildingRequirement: ICloneable {
    public string name;
    public double amount;

    public object Clone() {
        var br = new BuildingRequirement();
        br.name = this.name;
        br.amount = this.amount;
        return br;
    }
}

public class ConstructionManager : MonoBehaviour {
    [SerializeField]
    private bool instabuild = false;
    [SerializeField]
    private List<string> techRequirements = new List<string>();
    [SerializeField]
    private BuildingRequirement[] statRequirements;
    [SerializeField]
    private BuildingRequirement[] resourceRequirements;
    [SerializeField]
    private List<ConstructionReservation> reservations;
    [SerializeField]
    private BuildingRequirement[] unfilledResourceReqs;

    [SerializeField]
    [DontSaveField]
    private ActionProfile cachedActionProfile;

    [SerializeField]
    private List<MonoBehaviour> cachedComponents;

    private GameController _gameController;
    public GameController gameController {
        get {
            if (_gameController == null) {
                _gameController = GameController.Instance;
            }
            return _gameController;
        }
        set { _gameController = value; }
    }

    private GroundController _groundController;
    public GroundController groundController {
        get {
            if (_groundController == null) {
                _groundController = gameController.groundController;
            }
            return _groundController;
        }
        set { _groundController = value; }
    }


    public void Start() {
        var cloneList = new List<BuildingRequirement>();
        foreach (var req in resourceRequirements) {
            cloneList.Add((BuildingRequirement)req.Clone());
        }
        unfilledResourceReqs = cloneList.ToArray();
    }

    public void GhostGood() {
        var r = GetComponentsInChildren<MeshRenderer>();
        foreach (var q in r) {
            q.material.shader = Shader.Find("Custom/BuildingGhost");
            q.material.SetColor("_GhostColor", Color.green);
        }
    }

    public void GhostBad() {
        var r = GetComponentsInChildren<MeshRenderer>();
        foreach (var q in r) {
            q.material.shader = Shader.Find("Custom/BuildingGhost");
            q.material.SetColor("_GhostColor", Color.red);
        }
    }
    public void GhostBuilding() {
        var r = GetComponentsInChildren<MeshRenderer>();
        foreach (var q in r) {
            q.material.shader = Shader.Find("Custom/BuildingGhost");
            q.material.SetColor("_GhostColor", Color.white);
        }
    }

    public void UnGhost() {
        var r = GetComponentsInChildren<MeshRenderer>();
        foreach (var q in r) {
            q.material.shader = Shader.Find("Standard");
        }
    }

    public void FixedUpdate() {
        reservations.RemoveAll((r) => {
            return r.Released || r.Cancelled;
        });
    }

    public bool ConstructionFinished() {
        double neededResources = 0;
        foreach (BuildingRequirement req in unfilledResourceReqs) {
            neededResources += req.amount;
        }
        return neededResources <= 0;
    }

	public bool ElligibleToBuild() {
        TechManager tm = gameController.techmanager;
        foreach (var r in techRequirements) {
            if (!tm.TechResearched(r)) {
                return false;
            }
        }
        foreach (var r in statRequirements) {
            var statManager = gameController.statManager;
            if (statManager.Stat(r.name).Value < (decimal)r.amount) {
                return false;
            }
        }
        return true;
    }

    public bool IsBuildable(Vector3 position) {
        if (position.y <= groundController.waterLevel) {
            return false;
        }
        if (instabuild) {
            var availResources = gameController.GetAllAvailableResources();
            foreach (var r in resourceRequirements) {
                if (  !availResources.ContainsKey(r.name) 
                    || availResources[r.name] < r.amount) 
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void StartPlacement() {
        NeolithicObject no = GetComponent<NeolithicObject>();
        no.selectable = false;

        cachedActionProfile = no.actionProfile;
        no.actionProfile = (ActionProfile)Resources.Load("ActionProfiles/Empty");

        cachedComponents = new List<MonoBehaviour>();
        foreach (var r in GetComponents<Reservoir>()) {
            r.enabled = false;
            cachedComponents.Add(r);
        }
        foreach (var r in GetComponents<Warehouse>()) {
            r.enabled = false;
            cachedComponents.Add(r);
        }

        GhostBad();
    }

    public void StartConstruction() {
        if (instabuild) {
            foreach (var r in resourceRequirements) {
                var rp = new ResourceProfile(r.name, r.amount);
                if (!gameController.WithdrawFromAnyWarehouse(rp)) {
                    throw new Exception("Failed to build building, unable to withdraw "+r.name);
                }
            }

            FinishContruction();
        } else {
            NeolithicObject no = GetComponent<NeolithicObject>();
            no.actionProfile = (ActionProfile)Resources.Load("ActionProfiles/Constructable");
            GhostBuilding();
        }
    }

    public void FinishContruction() {
        NeolithicObject no = GetComponent<NeolithicObject>();
        no.selectable = true;
        no.actionProfile = cachedActionProfile;
        foreach (var r in cachedComponents) {
            r.enabled = true;
        }
        UnGhost();
        Destroy(this);
    }

    public bool GetJobReservation(ActorController actor) {
        var avails = gameController.GetAllAvailableResources();
        foreach (var kvp in avails) {
            string resourceTag = kvp.Key;
            double amount = kvp.Value;
            Debug.Log("Checking if I need " + amount + " " + resourceTag);
            double needed = GetNeededResource(resourceTag);
            Debug.Log("I need " + needed + " " + resourceTag);
            if (needed > 0) {
                Debug.Log("Making a ConstructionReservation");
                var res = actor.gameObject.AddComponent<ConstructionReservation>();
                reservations.Add(res);
                res.resourceTag = resourceTag;
                res.amount = 1;
                return true;
            }
        }
        return false;
    }

    public double GetNeededResource(string resourceTag) {
        double needed = 0;
        foreach (var requirement in unfilledResourceReqs) {
            if (requirement.name == resourceTag) {
                needed += requirement.amount;
            }
        }
        foreach (var res in reservations) {
            if (    res.resourceTag == resourceTag 
                && !res.Released 
                && !res.Cancelled)
            {
                needed -= res.amount;
            }
        }
        return needed;
    }

    /// <summary>
    /// Fills the given reservation and removes it from the list of unfilled resource requirements
    /// </summary>
    /// <param name="res"></param>
    public void FillReservation(ConstructionReservation res) {
        if (!reservations.Contains(res)) {
            throw new ArgumentException("Reservation does not belong to this construction object");
        }
        foreach (var requirement in unfilledResourceReqs) {
            if (requirement.name == res.resourceTag) {
                res.Released = true;
                requirement.amount -= res.amount;
                reservations.Remove(res);
                break;
            }
        }

        if (ConstructionFinished()) {
            FinishContruction();
        }
    }
}
