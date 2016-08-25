using UnityEngine;
using System.Collections;
using System;

public class GetConstructionJobOrder : BaseOrder {
    private Vector3 center, targetPosition;
    ConstructionManager manager;

    public GetConstructionJobOrder(ActorController a,
                                    ConstructionManager manager): base(a) 
    {
        center = a.transform.position;
        targetPosition = center;
        this.manager = manager;
    }

    public override void DoStep() {
        Vector3 diff = targetPosition - actor.transform.position;
        if (diff.magnitude <= actor.moveSpeed) {
            Debug.Log("Trying to get job reservation");
            if (manager.GetJobReservation(actor)) {
                Debug.Log("Got job reservation");
                this.completed = true;
                return;
            }
            float r = 5.0f;
            targetPosition = center + new Vector3(UnityEngine.Random.Range(-r, r), 0, UnityEngine.Random.Range(-r, r));
            targetPosition = actor.gameController.SnapToGround(targetPosition);
            diff = targetPosition - actor.transform.position;
        }
        actor.transform.position += diff * 0.08f * (actor.moveSpeed / diff.magnitude);
    }
}

public class CompleteConstructionReservation : BaseOrder {
    ConstructionManager manager;

    public CompleteConstructionReservation(ActorController actor, 
                                         ConstructionManager manager) 
        :base(actor)
    {
        this.manager = manager;
    }

    public override void DoStep() {
        if (actor.MoveTowards(manager.transform.position)) {
            ConstructionReservation res = actor.GetComponent<ConstructionReservation>();
            UnityEngine.Object.Destroy(actor.GetCarriedResource(res.resourceTag).gameObject);
            manager.FillReservation(res);
            completed = true;
        }
    }
}

/// <summary>
/// Testing order to transmute one resource to another
/// </summary>
public class ConstructOrder : StatefulSuperOrder {
    ConstructionManager manager;

    public ConstructOrder(ActorController a, NeolithicObject target) : base(a) {
        manager = target.GetComponent<ConstructionManager>();
        GoToState("getConstructionJob");
    }

    public override void Initialize() {
        Resource r = actor.GetCarriedResource();
        if (r != null)
            actor.DropCarriedResource();
    }

    protected override void CreateStates() {
        CreateState("getConstructionJob",
            () => new GetConstructionJobOrder(actor, manager),
            () => {
                if (manager.ConstructionFinished())
                    this.completed = true;
                else
                    GoToState("fetchResource");
            },
            null);
        CreateState("fetchResource",
            () => {
                var res = actor.GetComponent<ConstructionReservation>();
                return new FetchAvailableResourceOrder(actor, res.resourceTag, 1.0f);
            },
            () => GoToState("depositResource"),
            null);
        CreateState("depositResource",
            () => new CompleteConstructionReservation(actor, manager),
            () => {
                if (!manager.ConstructionFinished()) {
                    GoToState("getConstructionJob");
                } else {
                    this.completed = true;
                }
            },
            null);
    }
}