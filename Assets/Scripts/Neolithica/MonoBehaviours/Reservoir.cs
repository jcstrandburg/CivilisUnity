using System;
using System.Collections.Generic;
using AqlaSerializer;
using Neolithica.DependencyInjection;
using Neolithica.MonoBehaviours.Reservations;
using Tofu.Serialization;
using UnityEngine;

namespace Neolithica.MonoBehaviours {
    [SavableMonobehaviour(7)]
    public class Reservoir : MonoBehaviour {
        public ResourceKind resourceResourceKind;
        public double amount;
        public float regenRate;
        public double max;
        public string harvestStat=null;
        public List<Reservation> reservations = new List<Reservation>();

        [Inject, DontSave] public StatManager statManager;
        [Inject, DontSave] private GameFactoryBase FactoryBase;

        // Handles Start event
        void Start() {
            GetComponent<NeolithicObject>();
        }

        // Handles FixedUpdate event
        void FixedUpdate() {
            Regen(Time.fixedDeltaTime);
            UpdateReservations();
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
            if (reservations.Count == 0)
                return;

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
                throw new ArgumentException("Reservation not for this Reservoir", nameof(res));
            }
            if (res.Released || res.Cancelled) {
                Debug.Log(res);
                throw new ArgumentException("Invalid reservation!", nameof(res));
            }

            if (amount >= res.amount) {
                amount -= res.amount;
                res.Released = true;
                reservations.Remove(res);

                if (!String.IsNullOrEmpty(harvestStat)) {
                    IGameStat stat = statManager.Stat(harvestStat);
                    if (stat != null) {
                        stat.Add(res.amount);
                    } else {
                        Debug.Log($"Unable to resolve stat {harvestStat}");
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
            var r = FactoryBase.AddComponent<ResourceReservation>(go);
            r.resourceKind = this.resourceResourceKind;
            r.amount = amount;
            r.source = this.gameObject;
            reservations.Add(r);
            return r;
        }
    }
}
