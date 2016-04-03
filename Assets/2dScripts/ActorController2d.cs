using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActorController2d : CivilisObject {

	public bool idling = false;

	public Queue<ActorOrder2d> orderQueue = new Queue<ActorOrder2d>();

	void Start () {		
	}
	
	void FixedUpdate() {

		if ( orderQueue.Count > 0) {
			ActorOrder2d currentOrder = orderQueue.Peek();
			while (currentOrder != null && currentOrder.finished) {
				orderQueue.Dequeue();
				if ( orderQueue.Count > 0) {
					currentOrder = orderQueue.Peek();
				}
				else {
					currentOrder = null;
				}
			}

			if ( currentOrder != null) {
				currentOrder.DoStep();
			}
		}

		//handle idling if no valid orders left
		if ( orderQueue.Count == 0) {
			orderQueue.Enqueue(new IdleOrder2d(gameObject));
			idling = true;
		}
	}
	
	void Update () {
	}

	public void SetOrder(ActorOrder2d order) {
		CancelOrders ();
		orderQueue.Enqueue(order);
		idling = false;
	}

	public void EnqueueOrder(ActorOrder2d order) {
		if ( idling) {
			CancelOrders ();
			idling = false;
		}
		orderQueue.Enqueue(order);
	}

	public void CancelOrders() {
		while (orderQueue.Count > 0) {
			ActorOrder2d cancelMe = orderQueue.Dequeue();
			cancelMe.Cancel ();
		}
	}
}
