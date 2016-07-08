using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Reservoir : MonoBehaviour {
    [DontSaveField]
	public GameObject prefab;
	public Queue<Reservation> reservations = new Queue<Reservation>();

	void FixedUpdate() {
		while (reservations.Count > 0) {
			Reservation r = reservations.Peek();
			if (r.Released) {
				reservations.Dequeue();
			} else {
				r.Ready = true;
				break;
			}
		}
		GetComponent<NeolithicObject>().statusString = string.Format("{0}", reservations.Count);
	}

	public Reservation NewReservation(ActorController actor) {
		Reservation r = actor.gameObject.AddComponent<Reservation>();
		reservations.Enqueue(r);
		return r;
	}
}
