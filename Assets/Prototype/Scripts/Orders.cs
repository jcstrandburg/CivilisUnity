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

	public SimpleMoveOrder(ActorController a, Vector3 position) : base(a) {
		a.GetComponent<NeolithicObject>().statusString = "Moving to position";
		targetPosition = position;
	}

	public override void DoStep() {
        this.completed = actor.MoveTowards(targetPosition);
        
        /*
		Vector3 diff = targetPosition - actor.transform.position;
		if (diff.magnitude <= actor.moveSpeed) {
			actor.transform.position = targetPosition;
			this.completed = true;
		} else {
			actor.transform.position += diff * (actor.moveSpeed/diff.magnitude);
		}
        */
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
			GameObject res = UnityEngine.Object.Instantiate(prefab);
			res.transform.SetParent(actor.transform);
			res.transform.localPosition = new Vector3(0, 4.0f, 0);
			completed = true;
		}
	}
}

public class TempDumpOrder : BaseOrder {
	public TempDumpOrder(ActorController a) : base(a) {
	}
	
	public override void DoStep() {
		GameObject resource = null;
		foreach (Transform t in actor.transform) {
			if (t.tag == "Resource") { resource = t.gameObject; break; }
		}

		while (resource != null) {
            resource.GetComponent<Resource>().preserved = false;
			resource.transform.SetParent(actor.transform.parent);
			float range = 3.0f;
			Vector3 randomVector = new Vector3(UnityEngine.Random.Range(-range, range), .1f, UnityEngine.Random.Range(-range, range));
			resource.transform.position = actor.transform.position + randomVector;
			completed = true;

			resource = null;
			foreach (Transform t in actor.transform) {
				if (t.tag == "Resource") { resource = t.gameObject; break; }
			}
		}
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
		CreateState("seekStorage",  StartSeekStore, CompleteSeekStore, null);
		CreateState("dumpResource", StartDump, CompleteDump, null);
		GoToState("seekTarget");
	}
	
	private BaseOrder StartSeek() 	{ return new SimpleMoveOrder(actor, targetObj.transform.position); }	
	private void CompleteSeek() 	{ GoToState("reservationWait"); }	

	private BaseOrder StartReservationWait() 	{ tempres = reservoir.NewReservation(actor); return new WaitForReservationOrder(actor, tempres); }
	private void CompleteReservationWait()  	{ GoToState("getResource"); }

	private BaseOrder StartGet() { return new TempExtractOrder(actor, targetObj); }	
	private void CompleteGet() 	 { tempres.Released = true; GameObject.Destroy(tempres); tempres = null; GoToState("seekStorage"); }

	private BaseOrder StartSeekStore()	{ return new SimpleMoveOrder(actor, originPos); }	
	private void CompleteSeekStore() 	{ GoToState("dumpResource"); }

	private BaseOrder StartDump() 	{ return new TempDumpOrder(actor); }
	private void CompleteDump() 	{ GoToState("seekTarget"); }

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
                res.transform.SetParent(actor.transform);
                res.transform.localPosition = new Vector3(0, 4.0f, 0);
                completed = true;
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
	
//	public override void Cancel() {
//		base.Cancel();
//		if (tempres) {
//			tempres.Released = true;
//			GameObject.Destroy(tempres);
//		}
//	}
}

