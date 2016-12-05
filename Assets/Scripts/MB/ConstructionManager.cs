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
public class BuildingRequirement : ICloneable {
    public string name;
    public double amount;
    public double epsilon=0.01;
    public Comparison comparison = Comparison.GreaterOrEqual;

    public object Clone() {
        var br = new BuildingRequirement();
        br.name = this.name;
        br.amount = this.amount;
        return br;
    }

    public bool IsSatisfied(float compareAmount) {
        switch (comparison) {
        case Comparison.LessThan:
            return compareAmount < amount;
        case Comparison.LesserOrEqual:
            return compareAmount <= amount;
        case Comparison.Equal:
            return compareAmount >= (amount-epsilon) && compareAmount <= (amount + epsilon);
        case Comparison.GreaterOrEqual:
            return compareAmount >= amount;
        case Comparison.GreaterThan:
            return compareAmount > amount;
        default:
            throw new InvalidOperationException("Unhandled comparison value" + comparison);
        }
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

    private GameController gameController;
    public GameController GameController {
        get {
            if (gameController == null) {
                gameController = GameController.Instance;
            }
            return gameController;
        }
        set { gameController = value; }
    }

    private GroundController groundController;
    public GroundController GroundController {
        get {
            if (groundController == null) {
                groundController = GameController.GroundController;
            }
            return groundController;
        }
        set { groundController = value; }
    }

    // Handles Start event
    public void Start() {
        var cloneList = new List<BuildingRequirement>();
        foreach (var req in resourceRequirements) {
            cloneList.Add((BuildingRequirement)req.Clone());
        }
        unfilledResourceReqs = cloneList.ToArray();
    }

    // Handles FixedUpdate event
    public void FixedUpdate() {
        reservations.RemoveAll((r) => {
            return r.Released || r.Cancelled;
        });
    }

    /// <summary>
    /// Enables ghost shading with the bad/unplaceable color
    /// </summary>
    public void GhostGood() {
        var r = GetComponentsInChildren<MeshRenderer>();
        foreach (var q in r) {
            q.material.shader = Shader.Find("Custom/BuildingGhost");
            q.material.SetColor("_GhostColor", Color.green);
        }
    }

    /// <summary>
    /// Enables ghost shading with the good/placeable color
    /// </summary>
    public void GhostBad() {
        var r = GetComponentsInChildren<MeshRenderer>();
        foreach (var q in r) {
            q.material.shader = Shader.Find("Custom/BuildingGhost");
            q.material.SetColor("_GhostColor", Color.red);
        }
    }

    /// <summary>
    /// Enables ghost shading with the neutral color
    /// </summary>
    public void GhostNeutral() {
        var r = GetComponentsInChildren<MeshRenderer>();
        foreach (var q in r) {
            q.material.shader = Shader.Find("Custom/BuildingGhost");
            q.material.SetColor("_GhostColor", Color.white);
        }
    }

    /// <summary>
    /// Disables ghost shading
    /// </summary>
    public void UnGhost() {
        var r = GetComponentsInChildren<MeshRenderer>();
        foreach (var q in r) {
            q.material.shader = Shader.Find("Standard");
        }
    }

    /// <summary>
    /// Determins if the construction process is finished
    /// </summary>
    /// <returns>True if finished, else false</returns>
    public bool ConstructionFinished() {
        double neededResources = 0;
        foreach (BuildingRequirement req in unfilledResourceReqs) {
            neededResources += req.amount;
        }
        return neededResources <= 0;
    }

    /// <summary>
    /// Determins if this building can be placed at the given position. If this is an instabuild building resource requirements will be checked also
    /// </summary>
    /// <param name="position"></param>
    /// <returns>True if buildable, else false</returns>
    public bool IsBuildable(Vector3 position) {
        if (position.y <= GroundController.waterLevel) {
            return false;
        }
        if (instabuild) {
            var availResources = GameController.GetAllAvailableResources();
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

    /// <summary>
    /// Starts the building placement process, caching some components and entering building/blueprint mode
    /// </summary>
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

    /// <summary>
    /// Starts the construction process, disabling and caching some key components
    /// </summary>
    public void StartConstruction() {
        if (instabuild) {
            foreach (var r in resourceRequirements) {
                var rp = new ResourceProfile(r.name, r.amount);
                if (!GameController.WithdrawFromAnyWarehouse(rp)) {
                    throw new InvalidOperationException("Failed to build building, unable to withdraw "+r.name);
                }
            }

            FinishContruction();
        } else {
            NeolithicObject no = GetComponent<NeolithicObject>();
            no.actionProfile = (ActionProfile)Resources.Load("ActionProfiles/Constructable");
            GhostNeutral();
        }
    }

    /// <summary>
    /// Finalizes construction, re-enabling cached compenents and some other prep stuff
    /// </summary>
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

    /// <summary>
    /// Builds a new ConstructionReservation and attaches it to the given actor
    /// </summary>
    /// <param name="actor"></param>
    /// <returns>True on success, false on failure</returns>
    public bool GetJobReservation(ActorController actor) {
        var avails = GameController.GetAllAvailableResources();
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

    /// <summary>
    /// Gets the total resources still needed to complete construction
    /// </summary>
    /// <param name="resourceTag"></param>
    /// <returns>Resources needed to complete construction</returns>
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

    /// <summary>
    /// Determins if all tech and stat requirements are met for this building
    /// </summary>
    /// <param name="statManager"></param>
    /// <param name="techManager"></param>
    /// <returns>True if requirements met, false otherwise</returns>
    public bool RequirementsMet(StatManager statManager, TechManager techManager) {
        foreach (var requirement in statRequirements) {
            var statname = requirement.name;
            var stat = statManager.Stat(statname);
            if (!requirement.IsSatisfied((float)stat.Value)) {
                return false;
            }
        }

        foreach (var tech in techRequirements) {
            if (!techManager.TechResearched(tech)) {
                return false;
            }
        }

        return true;
    }
}
