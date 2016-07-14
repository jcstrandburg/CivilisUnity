using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// The base for all actor orders
/// </summary>
[CustomSerialize]
public abstract class BaseOrder {
	public ActorController actor;
	public bool completed;
	public bool cancelled;
	public bool failed;

	public bool Done {
		get {
			return completed || cancelled || failed;
		}
	}

	public BaseOrder(ActorController a) {
		actor = a;
		completed = cancelled = failed = false;
	}

	/// <summary>
	/// Does a single step for this order
	/// </summary>
	public abstract void DoStep();

	/// <summary>
	/// Cancels this order, freeing any resources as appropriate
	/// </summary>
	public virtual void Cancel() {
		cancelled = true;
	}
}

/// <summary>
/// A simple stateless move order
/// </summary>
public class SimpleMoveOrder : BaseOrder {
	public Vector3 targetPosition;
    float proximity;

	public SimpleMoveOrder(ActorController a, Vector3 position, float proximity=0.0f) : base(a) {
		a.GetComponent<NeolithicObject>().statusString = "Moving to position";
        this.proximity = proximity;
		targetPosition = position;
	}

	public override void DoStep() {
        this.completed = actor.MoveTowards(targetPosition);
        Vector3 diff = targetPosition - actor.transform.position;
        if (diff.magnitude < proximity) {
            this.completed = true;
        }
    }
}

public class SimpleWithdrawOrder : BaseOrder {
    public SimpleWithdrawOrder(ActorController a) : base(a) {
    }

    public override void DoStep() {
        Warehouse w = actor.resourceReservation.source.GetComponent<Warehouse>();
        try {
            w.WithdrawReservation(actor.resourceReservation);
            string tag = actor.resourceReservation.resourceTag;
            GameObject r = GameController.instance.CreateResourcePile(tag);
            actor.PickupResource(r);
            this.completed = true;
        } catch (Exception e) {
            Debug.Log("SimpleWithdrawOrder failed to withdraw with exception");
            Debug.Log(e);
            this.failed = true;
        }
    }
}

public class IdleOrder : BaseOrder {
	private Vector3 center, targetPosition;

	public IdleOrder(ActorController a) : base(a) {
		a.GetComponent<NeolithicObject>().statusString = "Idling";
		center = targetPosition = a.transform.position;
	}

	public override void DoStep() {
		Vector3 diff = targetPosition - actor.transform.position;
		if (diff.magnitude <= actor.moveSpeed) {
			float r = 5.0f;
			targetPosition = center + new Vector3(UnityEngine.Random.Range(-r, r), 0, UnityEngine.Random.Range(-r, r));
            targetPosition = GameController.instance.SnapToGround(targetPosition);
			diff = targetPosition - actor.transform.position;
		}
		actor.transform.position += diff * 0.08f * (actor.moveSpeed/diff.magnitude);
	}
}

public class WaitForReservationOrder : IdleOrder {
	private Reservation reservation;

	public WaitForReservationOrder(ActorController a, Reservation r) : base(a) {
		a.GetComponent<NeolithicObject>().statusString = "Waiting for reservation";
		reservation = r;
	}

	public override void DoStep() {
		base.DoStep();
		if (reservation.Ready) {
			completed = true;
		}
	}
}

/// <summary>
/// Utility struct for stateful super orders
/// </summary>
[Serializable]
public struct OrderStateInfo {
	public Func<BaseOrder> startState;
	public Action completeState;
	public Action failState;

	public OrderStateInfo(Func<BaseOrder> start, Action complete, Action fail) {
		startState = start; completeState = complete; failState=fail;
	}
}

/// <summary>
/// Base class for stateful orders with multiple steps
/// </summary>
public abstract class StatefulSuperOrder : BaseOrder {
	public BaseOrder currentOrder = null;
	public string currentState;
    [DontSaveField]
    private IDictionary<string, OrderStateInfo> states = null;

    protected IDictionary<string, OrderStateInfo> States {
        get {
            if (states == null) {
                states = new Dictionary<string, OrderStateInfo>();
                CreateStates();
            }
            return states;
        }
    }

	public StatefulSuperOrder(ActorController a): base(a) {
	}

    protected abstract void CreateStates();

	public void CreateState(string stateName, Func<BaseOrder> startState, Action completeState, Action failState) {
		OrderStateInfo info = new OrderStateInfo(startState, completeState, failState);
		states.Add(stateName, info);
	}

	public override void DoStep() {
		if (currentOrder != null) {
			currentOrder.DoStep ();

			//check to see if order is done somehow
			if (currentOrder.Done) {
				OrderStateInfo info = States[currentState];
				if (currentOrder.completed) {
					if (info.completeState != null) {
						info.completeState ();
					} else {
						this.failed = true;
						Debug.Log ("No complete transition available for state: " + currentState);
					}
				} else if (currentOrder.failed) {
					if (info.failState != null) {
						info.failState ();
					} else {
						this.failed = true;
						Debug.Log ("No failure transition available for state: " + currentState);
					}
				} else if (currentOrder.cancelled) {
					throw new Exception ("Order exectution cannot continue when sub order is cancelled!");
				}
			}
		} else {
			Debug.Log ("currentOrder is null!");
			this.failed = true;
		}
	}

	/// <summary>Changes to the given state</summary>
	public void GoToState(string state) {
		if (States.ContainsKey(state)) {
			OrderStateInfo info = States[state];
			currentState = state;
			currentOrder = info.startState();
		} else {
			throw new ArgumentOutOfRangeException("Nonexistant order state: "+state);
		}
	}
}

/// <summary>
/// Order to fetch the given resource from any available warehouse
/// </summary>
public class TempFetchOrder : StatefulSuperOrder {
    string resourceType;
    float amount;

    public TempFetchOrder(ActorController a, string resourceType, float amount): base(a) {
        this.resourceType = resourceType;
        this.amount = amount;
        GoToState("getReservation");
    }

    protected override void CreateStates() {
        CreateState("getReservation",
                    () => new TempReserveResourceOrder(actor, resourceType, amount),
                    () => GoToState("gotoWarehouse"),
                    null);
        CreateState("gotoWarehouse",
            () => new SimpleMoveOrder(actor, actor.resourceReservation.source.transform.position, 2.0f),
            () => GoToState("withdraw"),
            null);
        CreateState("withdraw",
            () => new SimpleWithdrawOrder(actor),
            () => this.completed = true,
            null);
    }
}

/// <summary>
/// Order to extract resources from the given target object
/// </summary>
public class TempExtractOrder : BaseOrder {
	int progress = 0;
	NeolithicObject target;

	public TempExtractOrder(ActorController a, NeolithicObject targetObj) : base(a) {
		a.GetComponent<NeolithicObject>().statusString = "Extracting resource";
		target = targetObj;
	}
	
	public override void DoStep() {
		progress++;
		if (progress == 90) {
			Reservoir reservoir = target.GetComponent<Reservoir>();

            //this code is ugly and temporary, fix it later
            Resource r = reservoir.prefab.GetComponent<Resource>();
            GameObject res = GameController.instance.CreateResourcePile(r.typeTag);
            actor.PickupResource(res);
            completed = true;
		}
	}
}

/// <summary>
/// Order to reserve the given resrouces from any available warehouse
/// </summary>
public class TempReserveResourceOrder: BaseOrder {
    string resourceType;
    float amount;

    public TempReserveResourceOrder(ActorController a, string resourceType, float amount) : base(a) {
        this.resourceType = resourceType;
        this.amount = amount;
    }

    public override void DoStep() {
        if (GameController.instance.ReserveWarehouseResources(actor, resourceType, amount)) {
            completed = true;
        }
    }
}

/// <summary>
/// Order to reserve storage for the current carried resource in any available warehouse
/// </summary>
public class TempReserveStorageOrder : BaseOrder {
    Resource resource;

    public TempReserveStorageOrder(ActorController a) : base(a) {
        resource = a.GetCarriedResource();
        if (!resource) {
            failed = true;
        }
    }

    public override void DoStep() {
        string type = resource.typeTag;
        float amount = resource.amount;
        if (GameController.instance.ReserveStorage(actor, type, amount)) {
            Debug.Log("Reserved storage");
            completed = true;
        } else {
            failed = true;
        }
    }
}

/// <summary>
/// Order to store the resources for the given StorageReservation in any available warehouse
/// </summary>
public class TempStoreReservationOrder : BaseOrder {
    StorageReservation res;

    public TempStoreReservationOrder(ActorController a, StorageReservation r) : base(a) {
        res = r;
    }

    public override void DoStep() {
        if (actor.MoveTowards(res.warehouse.transform.position)) {
            res.warehouse.DepositReservation(res);
            GameObject.Destroy(actor.GetCarriedResource(res.resourceTag).gameObject);
            res.Released = true;
            this.completed = true;
        }
    }
}

/// <summary>
/// Super order to find, seek, and utilize storage for the currently carried resource
/// </summary>
public class TempStoreOrder : StatefulSuperOrder {
    public TempStoreOrder(ActorController a): base(a) {
        GoToState("getReservation");
    }

    protected override void CreateStates() {
        CreateState ("getReservation", 
			() => new TempReserveStorageOrder (actor),
			() => GoToState ("seekStorage"),
			() => {
				Debug.Log("Couldn't get reservation, dumping!");
				GoToState("dump");
			});
		CreateState ("dump",
			() => new TempDumpOrder (actor),
			() => this.completed = true,
			null);
		CreateState ("seekStorage", 
			() => new SimpleMoveOrder (actor, actor.storageReservation.warehouse.transform.position, 2.0f),
			() => GoToState ("reservationWait"),
			null);
		CreateState ("reservationWait", 
			()=>new WaitForReservationOrder (actor, actor.storageReservation),
			()=>GoToState ("deposit"),
			null);
		CreateState ("deposit", 
			() => new TempStoreReservationOrder (actor, actor.storageReservation),
			() => this.completed = true,
			null);
    }
}

/// <summary>
/// Order to dump the currently carried resource on the ground
/// </summary>
public class TempDumpOrder : BaseOrder {
    GameObject dump;

	public TempDumpOrder(ActorController a) : base(a) {
        dump = GameObject.Find("DumpingGround");
	}
	
	public override void DoStep() {
        try {
            if (actor.MoveTowards(dump.transform.position, 1.1f)) {
                actor.DropCarriedResource();
                this.completed = true;
            }
        } catch (Exception e) {
            Debug.Log("Exception in TempDumpOrder");
            Debug.Log(e);
            this.failed = true;
        }        
	}
}


/// <summary>
/// Order to generate spirit
/// </summary>
public class MeditateOrder : BaseOrder {
    public MeditateOrder(ActorController a, NeolithicObject target) : base(a) {
    }

    public override void DoStep() {
        GameController.instance.spirit += 0.03f;
    }
}


/// <summary>
/// Testing order to transmute one resource to another
/// </summary>
public class TransmuteOrder : StatefulSuperOrder {
    string fromTag;
    string toTag;
    NeolithicObject target;

    public TransmuteOrder(ActorController a, NeolithicObject target, string fromTag, string toTag) : base(a) {
        this.fromTag = fromTag;
        this.toTag = toTag;
        this.target = target;
        GoToState("getSourceMaterial");
    }

    protected override void CreateStates() {
        CreateState("getSourceMaterial",
			()=>new TempFetchOrder(actor, fromTag, 1.0f),
			()=>GoToState("gotoWorkspace"),
			null);
        CreateState("gotoWorkspace",
			()=>new SimpleMoveOrder(actor, target.transform.position, 2.0f),
			()=>GoToState("doTransmute"),
			null);
        CreateState("doTransmute",
			()=>new TempConvertOrder(actor, fromTag, toTag),
			()=>GoToState("storeProduct"),
			null);
        CreateState("storeProduct",
			()=>new TempStoreOrder(actor),
			()=>GoToState("getSourceMaterial"),
			null);
    }
}


/// <summary>
/// Simple order to convert a carried resource of one type to another type
/// </summary>
public class TempConvertOrder : BaseOrder {
    Resource sourceResource;
    string toTag;

    public TempConvertOrder (ActorController a, string fromTag, string toTag) : base(a) {
        Resource r = a.GetCarriedResource();
        if (r.typeTag != fromTag) {
            Debug.Log("Actor does not have resource " + fromTag + " to convert");
            this.failed = true;
        }
        sourceResource = r;
        this.toTag = toTag;
    }

    public override void DoStep() {
        GameObject newResource = GameController.instance.CreateResourcePile(toTag);
        Resource r = newResource.GetComponent<Resource>();
        r.amount = sourceResource.amount;
        actor.PickupResource(newResource);
        sourceResource.transform.SetParent(null);
        UnityEngine.Object.Destroy(sourceResource.gameObject);
        this.completed = true;
    }
}

/// <summary>
/// Simple order to seek, reserve, and extract resources from the given target
/// </summary>
public class TempHarvestOrder : StatefulSuperOrder {
	NeolithicObject targetObj;
	Reservoir reservoir;
	Reservation tempres;

    public TempHarvestOrder(ActorController a, NeolithicObject target) : base(a) {
        targetObj = target;
        reservoir = target.GetComponent<Reservoir>();
        GoToState("seekTarget");
    }

    protected override void CreateStates() {
        CreateState("seekTarget",
			()=>new SimpleMoveOrder(actor, targetObj.transform.position),
			()=>GoToState("reservationWait"),
			null);
		CreateState("reservationWait", 
			()=>{ 
				tempres = reservoir.NewReservation(actor); 
				return new WaitForReservationOrder(actor, tempres); 
			},
			()=>GoToState("getResource"),
			null);
		CreateState("getResource",
			()=>new TempExtractOrder(actor, targetObj),	
			()=>{ 
				tempres.Released = true; 
				GameObject.Destroy(tempres); 
				tempres = null; 
				GoToState("storeContents"); 
			},
			null);
		CreateState("storeContents",
			()=>new TempStoreOrder(actor),
			()=>GoToState("seekTarget"),
			null);
	}

	public override void Cancel() {
		base.Cancel();
		if (tempres) {
			tempres.Released = true;
			GameObject.Destroy(tempres);
		}
	}
}
	
public class TempSlaughterOrder : BaseOrder {
	int progress = 0;
	Herd herd;
	
	public TempSlaughterOrder(ActorController a, Herd targetHerd) : base(a) {
		a.GetComponent<NeolithicObject>().statusString = "Killing snorgle";
		herd = targetHerd;
	}
	
	public override void DoStep() {
		progress++;
		if (progress >= 60) {
            if (herd.KillAnimal()) {
                GameObject prefab = herd.resourcePrefab;
                GameObject res = UnityEngine.Object.Instantiate(prefab);
                actor.PickupResource(res);
                this.completed = true;
            } else {
                progress /= 2;
            }
		}
	}
}

public class TempHuntOrder : StatefulSuperOrder {
    Herd herd;

    public TempHuntOrder(ActorController a, Herd herd) : base(a) {
        this.herd = herd;
        GoToState("seekTarget");
    }

    protected override void CreateStates() {
        CreateState("seekTarget", () => new SimpleMoveOrder(actor, herd.rabbit.transform.position), ()=>GoToState("getResource"), null);
		CreateState("getResource",  ()=> new TempSlaughterOrder(actor, herd), ()=>GoToState("storeResource"),  null);
		CreateState("storeResource",  ()=> new TempStoreOrder(actor), ()=>GoToState("seekTarget"), null);
	}
}

