using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Reservoir : MonoBehaviour {
    public string resourceTag;
    public double amount;
    public float regenRate;
    public double max;
	public List<Reservation> reservations = new List<Reservation>();
    private NeolithicObject nobject;

    public void Regen(float time) {
        amount += (double)(time * regenRate);
        //clamp
        amount = (amount < 0 ? 0 : (amount > max ? max : amount));
    }

    public double GetAvailableContents() {
        double avail = amount;
        foreach (ResourceReservation r in reservations) {
            avail -= r.amount;
        }
        return avail >= 0 ? avail : 0;
    }

    public void UpdateReservations() {
        //reservations.RemoveAll((r) => { return r.Released || r.Cancelled; });
        double availAmount = amount;
        foreach (ResourceReservation res in reservations) {
            if (availAmount > res.amount) {
                availAmount -= res.amount;
                res.Ready = true;
            } else {
                return;
            }
        }
    }

    public bool WithdrawReservation(ResourceReservation res) {
        if (res.source != this.gameObject) {
            Debug.Log(res);
            throw new ArgumentException("Reservation not for this Reservoir");
        }
        if (res.Released || res.Cancelled) {
            Debug.Log(res);
            throw new ArgumentException("Invalid reservation!");
        }

        if (amount >= res.amount) {
            amount -= res.amount;
            res.Released = true;
            reservations.Remove(res);
            return true;
        } else {
            return false;
        }
    }

	void FixedUpdate() {
        Regen(Time.fixedDeltaTime);
        UpdateReservations();
        nobject.statusString = string.Format("{0} reservations, {1} {2}", reservations.Count, amount.ToString("F1"), resourceTag);
	}

    void Start() {
        nobject = GetComponent<NeolithicObject>();
    }

	public ResourceReservation NewReservation(GameObject go, double amount) {
        ResourceReservation r = go.AddComponent<ResourceReservation>();
        r.resourceTag = this.resourceTag;
        r.amount = amount;
        r.source = this.gameObject;
		reservations.Add(r);
		return r;
	}
}
