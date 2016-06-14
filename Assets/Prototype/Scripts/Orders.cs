using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// The base for all actor orders
/// </summary>
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

	public abstract void DoStep();
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
[System.Serializable]
public class StatefulSuperOrder : BaseOrder {
	public BaseOrder currentOrder = null;
	public string currentState;
	public IDictionary<string, OrderStateInfo> states = new Dictionary<string, OrderStateInfo>();

	public StatefulSuperOrder(ActorController a): base(a) {
	}

	public void CreateState(string stateName, Func<BaseOrder> startState, Action completeState, Action failState) {
		OrderStateInfo info = new OrderStateInfo(startState, completeState, failState);
		states.Add(stateName, info);
	}

	public override void DoStep() {
		if (currentOrder != null) {
			currentOrder.DoStep();

			//check to see if order is done somehow
			if (currentOrder.Done) {
				OrderStateInfo info = states[currentState];
				if (currentOrder.completed) {
					if (info.completeState != null) {
						info.completeState();
					} else {
						Debug.Log("No complete transition available for state: "+currentState);
					}
				} else if (currentOrder.failed) {
					if (info.failState != null) {
						info.failState();
					} else {
						Debug.Log("No failure transition available for state: "+currentState);
					}
				}
			}
		}
	}

	/// <summary>Changes to the given state</summary>
	public void GoToState(string state) {
		if (states.ContainsKey(state)) {
			OrderStateInfo info = states[state];
			currentState = state;
			currentOrder = info.startState();
		} else {
			throw new Exception("Nonexistant order state: "+state);
		}
	}
}

public class TempFetchOrder : StatefulSuperOrder {
    string resourceType;
    float amount;

    public TempFetchOrder(ActorController a, string resourceType, float amount): base(a) {
        this.resourceType = resourceType;
        this.amount = amount;
        CreateState("getReservation", StartReserve, CompleteReserve, null);
        CreateState("gotoWarehouse", StartApproachWarehouse, CompleteApproachWarehouse, null);
        CreateState("withdraw", StartWithdraw, CompleteWithdraw, null);
        GoToState("getReservation");
    }

    private BaseOrder StartReserve() { return new TempReserveResourceOrder(actor, resourceType, amount); }
    private void CompleteReserve() { GoToState("gotoWarehouse"); }

    private BaseOrder StartApproachWarehouse() {
        ResourceReservation res = actor.resourceReservation;
        return new SimpleMoveOrder(actor, res.source.transform.position, 2.0f);
    }
    private void CompleteApproachWarehouse() { GoToState("withdraw"); }

    private BaseOrder StartWithdraw() { return new SimpleWithdrawOrder(actor); }
    private void CompleteWithdraw() { this.completed = true; }
}

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
			GameObject prefab = reservoir.prefab;
			//GameObject res = UnityEngine.Object.Instantiate(prefab);

            //this code is ugly and temporary, fix it later
            Resource r = reservoir.prefab.GetComponent<Resource>();
            GameObject res = GameController.instance.CreateResourcePile(r.typeTag);
            actor.PickupResource(res);
            completed = true;
		}
	}
}

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

public class TempStoreOrder : StatefulSuperOrder {
    public TempStoreOrder(ActorController a): base(a) {
        CreateState("getReservation", StartReserve, CompleteReserve, FailReserve);
        CreateState("dump", StartDump, CompleteDump, null);
        CreateState("seekStorage", StartSeekStorage, CompleteSeekStorage, null);
        CreateState("reservationWait", StartReservationWait, CompleteReservationWait, null);
        CreateState("deposit", StartStore, CompleteStore, null);
        GoToState("getReservation");
    }

    private BaseOrder StartReserve() { return new TempReserveStorageOrder(actor); }
    private void CompleteReserve() { GoToState("seekStorage"); }
    private void FailReserve() { Debug.Log("Couldn't get reservation, dumping!");  GoToState("dump"); }

    private BaseOrder StartDump() { return new TempDumpOrder(actor); }
    private void CompleteDump() { completed = true;  }

    private BaseOrder StartSeekStorage() { return new SimpleMoveOrder(actor, actor.storageReservation.warehouse.transform.position, 2.0f); }
    private void CompleteSeekStorage() { GoToState("reservationWait"); }

    private BaseOrder StartReservationWait() { return new WaitForReservationOrder(actor, actor.storageReservation); }
    private void CompleteReservationWait() { GoToState("deposit"); }

    private BaseOrder StartStore() { return new TempStoreReservationOrder(actor, actor.storageReservation); }
    private void CompleteStore() { completed = true; }

    public override void Cancel() {
        base.Cancel();
    }
}

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

public class MeditateOrder : BaseOrder {
    public MeditateOrder(ActorController a, NeolithicObject target) : base(a) {
    }

    public override void DoStep() {
        GameController.instance.spirit += 0.01f;
    }
}

public class TransmuteOrder : StatefulSuperOrder {
    string fromTag;
    string toTag;
    NeolithicObject target;

    public TransmuteOrder(ActorController a, NeolithicObject target, string fromTag, string toTag): base(a) {
        this.fromTag = fromTag;
        this.toTag = toTag;
        this.target = target;
        CreateState("getSourceMaterial", StartGetMaterial, CompleteSeekMaterial, null);
        CreateState("gotoWorkspace", StartGotoWorkspace, CompleteGotoWorkspace, null);
        CreateState("doTransmute", StartDoTransmute, CompleteDoTransmute, null);
        CreateState("storeProduct", StartStoreProduct, CompleteStoreProduct, null);
        GoToState("getSourceMaterial");
    }

    private BaseOrder StartGetMaterial() { return new TempFetchOrder(actor, fromTag, 1.0f); }
    private void CompleteSeekMaterial() { GoToState("gotoWorkspace"); }

    private BaseOrder StartGotoWorkspace() { return new SimpleMoveOrder(actor, target.transform.position, 2.0f); }
    private void CompleteGotoWorkspace() { GoToState("doTransmute"); }

    private BaseOrder StartDoTransmute() { return new TempConvertOrder(actor, fromTag, toTag); }
    private void CompleteDoTransmute() { GoToState("storeProduct"); }

    private BaseOrder StartStoreProduct() { return new TempStoreOrder(actor); }
    private void CompleteStoreProduct() { GoToState("getSourceMaterial"); }
}

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

public class TempHarvestOrder : StatefulSuperOrder {
	Vector3 originPos;
	NeolithicObject targetObj;
	Reservoir reservoir;
	Reservation tempres;

	public TempHarvestOrder(ActorController a, NeolithicObject target): base(a) {
		originPos = a.transform.position;
		targetObj = target;
		reservoir = target.GetComponent<Reservoir>();
		CreateState("seekTarget",   StartSeek, CompleteSeek, null);
		CreateState("reservationWait", StartReservationWait, CompleteReservationWait, null);
		CreateState("getResource",  StartGet,  CompleteGet,  null);
		CreateState("storeContents", StartStore, CompleteStore, null);
		GoToState("seekTarget");
	}
	
	private BaseOrder StartSeek() 	{ return new SimpleMoveOrder(actor, targetObj.transform.position); }	
	private void CompleteSeek() 	{ GoToState("reservationWait"); }	

	private BaseOrder StartReservationWait() 	{ tempres = reservoir.NewReservation(actor); return new WaitForReservationOrder(actor, tempres); }
	private void CompleteReservationWait()  	{ GoToState("getResource"); }

	private BaseOrder StartGet() { return new TempExtractOrder(actor, targetObj); }	
	private void CompleteGet() 	 { tempres.Released = true; GameObject.Destroy(tempres); tempres = null; GoToState("storeContents"); }

	private BaseOrder StartSeekStore()	{ return new SimpleMoveOrder(actor, originPos); }	
	private void CompleteSeekStore() 	{ GoToState("dumpResource"); }

	private BaseOrder StartStore() 	{ return new TempStoreOrder(actor); }
	private void CompleteStore() 	{ GoToState("seekTarget"); }

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
	Vector3 originPos;
	Herd target;
	//Reservoir reservoir;
	//Reservation tempres;
	
	public TempHuntOrder(ActorController a, Herd herd): base(a) {
		originPos = a.transform.position;
		target = herd;
		CreateState("seekTarget",   StartSeek, CompleteSeek, null);
		//CreateState("reservationWait", StartReservationWait, CompleteReservationWait, null);
		CreateState("getResource",  StartGet,  CompleteGet,  null);
		CreateState("seekStorage",  StartSeekStore, CompleteSeekStore, null);
		CreateState("dumpResource", StartDump, CompleteDump, null);
		GoToState("seekTarget");
	}
	
	private BaseOrder StartSeek() 	{ return new SimpleMoveOrder(actor, target.rabbit.transform.position); }	
	private void CompleteSeek() 	{ GoToState("getResource"); }	
	
	//private BaseOrder StartReservationWait() 	{ tempres = reservoir.NewReservation(actor); return new WaitForReservationOrder(actor, tempres); }
	//private void CompleteReservationWait()  	{ GoToState("getResource"); }
	
	private BaseOrder StartGet() { return new TempSlaughterOrder(actor, target); }	
	private void CompleteGet() 	 { GoToState("seekStorage"); }
	
	private BaseOrder StartSeekStore()	{ return new SimpleMoveOrder(actor, originPos); }	
	private void CompleteSeekStore() 	{ GoToState("dumpResource"); }
	
	private BaseOrder StartDump() 	{ return new TempDumpOrder(actor); }
	private void CompleteDump() 	{ GoToState("seekTarget"); }
}

