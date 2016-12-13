﻿using UnityEngine;
using System.Collections.Generic;
using System;

public class Reservoir : MonoBehaviour {
    public Resource.Type resourceType;
    public double amount;
    public float regenRate;
    public double max;
    public string harvestStat=null;
	public List<Reservation> reservations = new List<Reservation>();
    private NeolithicObject nobject;

    [Inject]
    public StatManager statManager;
    [Inject]
    public GameFactory Factory { get; set; }

    // Handles Start event
    void Start() {
        nobject = GetComponent<NeolithicObject>();
    }

    // Handles FixedUpdate event
    void FixedUpdate() {
        Regen(Time.fixedDeltaTime);
        UpdateReservations();
        nobject.statusString = string.Format("{0} reservations, {1} {2}", reservations.Count, amount.ToString("F1"), resourceType);
    }

    /// <summary>
    /// Regenerates resources based on the given time factor
    /// </summary>
    /// <param name="time"></param>
    public void Regen(float time) {
        amount += (double)(time * regenRate);
        amount = Math.Min(Math.Max(0, amount), max);
    }

    /// <summary>
    /// Gets all available contents for this reservoir
    /// </summary>
    /// <returns></returns>
    public double GetAvailableContents() {
        double avail = amount;
        foreach (ResourceReservation r in reservations) {
            avail -= r.amount;
        }
        return avail >= 0 ? avail : 0;
    }

    /// <summary>
    /// Updates ResourceReservations, marking them ready as appropriate
    /// </summary>
    public void UpdateReservations() {
        reservations.RemoveAll((r) => r.Cancelled || r.Released);
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

    /// <summary>
    /// Withdraws the given reservation from this reservoir
    /// </summary>
    /// <param name="res"></param>
    /// <exception cref="System.ArgumentException">Thrown when an invalid reservation is passed</exception>
    /// <returns>True on success, false if the reservation is not fulfillable</returns>
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

            if (!String.IsNullOrEmpty(harvestStat)) {
                GameStat stat = statManager.Stat(harvestStat);
                if (stat != null) {
                    stat.Add(res.amount);
                } else {
                    Debug.Log("Unable to resolve stat " + harvestStat);
                }                
            }

            return true;
        } else {
            return false;
        }
    }

    /// <summary>
    /// Creates a new resource reservation and adds it to the given GameObject, returning the reservation
    /// </summary>
    /// <param name="go"></param>
    /// <param name="amount"></param>
    /// <returns>The reservation</returns>
	public ResourceReservation NewReservation(GameObject go, double amount) {
        var r = Factory.AddComponent<ResourceReservation>(go);
        r.type = this.resourceType;
        r.amount = amount;
        r.source = this.gameObject;
		reservations.Add(r);
		return r;
	}
}
