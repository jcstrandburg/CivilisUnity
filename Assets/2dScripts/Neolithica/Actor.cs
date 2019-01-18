using System.Collections.Generic;
using Neolithica.Orders;
using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Logistics;
using Neolithica.MonoBehaviours.Reservations;
using Neolithica.Orders.Simple;
using UnityEditor;
using UnityEngine;

namespace Neolithica {
    public class Actor : MonoBehaviour, IOrderable {
        public float moveSpeed;
        public BaseOrder currentOrder;
        public bool idle;
        public double health = 1.0;
        public LogisticsActor logisticsActor;

        public StorageReservation StorageReservation => GetComponentInChildren<StorageReservation>();
        public ResourceReservation ResourceReservation => GetComponentInChildren<ResourceReservation>();

        private readonly Queue<BaseOrder> orderQueue = new Queue<BaseOrder>();

        Transform IOrderable.Transform => transform;
        GameController IOrderable.GameController => null;
        GameObject IOrderable.GameObject => gameObject;

        // Handles Start event
        public void Start () {
            logisticsActor = GetComponent<LogisticsActor>();
        }

        // Handles OnDestroy event
        public void OnDestroy() {
            currentOrder?.Cancel();

            foreach (Reservation r in GetComponents<Reservation>())
                r.Cancelled = true;
        }

        // Handles FixedUpdate event
        public void FixedUpdate() {
            ConsumeFood();

            if (health <= 0.0) {
                Destroy(gameObject);
                return;
            }

            if (currentOrder == null || currentOrder.Done) {
                if (orderQueue.Count > 0) {
                    currentOrder = DequeueOrder();
                    idle = false;
                } else {
                    currentOrder = new IdleOrder2D(this);
                    idle = true;
                }
            }

            currentOrder.Update(this);
        }

        private void ConsumeFood() {
           if (logisticsActor == null)
                return;

            double feedMe = Time.fixedDeltaTime * 0.025f;
            LogisticsManager logisticsManager = logisticsActor.logisticsManager;
            LogisticsNetwork network = logisticsManager ? logisticsManager.FindNearestNetwork(transform.position) : null;

            if (network != null && network.Foodbuffer > feedMe) {
                network.Foodbuffer -= feedMe;
                health = System.Math.Min(1, health + feedMe);
            } else {
                health -= feedMe;
            }
        }

        /// <summary>
        /// Gets and removes the first order from the order queue
        /// </summary>
        /// <returns>The first order from the queue</returns>
        public BaseOrder DequeueOrder() => orderQueue.Dequeue();

        /// <summary>Adds an order to the pending order queue</summary>
        public void EnqueueOrder(BaseOrder o) {
            if (idle)
                OverrideOrder(o);
            else
                orderQueue.Enqueue(o);
        }

        /// <summary>Clears the order queue and sets the current order</summary>
        public void OverrideOrder(BaseOrder o) {
            orderQueue.Clear();
            currentOrder?.Cancel();

            idle = false;
            currentOrder = o;
        }

        /// <summary>Moves towards the given target position, returning true if the position has been reached at the end of the move</summary>
        public bool MoveTowards(Vector3 targetPosition, float moveRatio = 1.0f) {
            Vector3 diff = targetPosition - transform.position;

            if (diff.magnitude <= moveSpeed * moveRatio) {
                transform.position = targetPosition;
                return true;
            }

            transform.position += diff * (moveSpeed / diff.magnitude) * moveRatio;
            return false;
        }

        /// <summary>
        /// Finds and returned the first found child object that has the tag "Resource" and has a Resource component
        /// </summary>
        /// <param name="resourceKind"></param>
        /// <returns>A Resource object</returns>
        public Resource GetCarriedResource(ResourceKind? resourceKind = null) {
            foreach (Transform t in transform) {
                if (t.gameObject == null || t.tag != "Resource") continue;
                var r = t.GetComponent<Resource>();
                if (resourceKind == null || r.resourceKind == resourceKind) {
                    return r;
                }
            }
            return null;
        }
    
        /// <summary>
        /// Drops the currently carried resource on the ground as a resource pile
        /// </summary>
        public void DropCarriedResource() {
            Resource r = GetCarriedResource();
            if (!r) {
                return;
            }

            const float range = 3.5f;
            var randomVector = new Vector3(Random.Range(-range, range), Random.Range(-range, range));
            r.transform.position = transform.position + randomVector;
            r.SetDown();
        }

        /// <summary>
        /// Picks up the given resource. It is assumed that the give
        /// </summary>
        /// <param name="res"></param>
        public void PickupResource(Resource res) {
            res.transform.SetParent(transform);
            res.transform.localPosition = new Vector3(0, 4.0f, 0);
            res.Pickup();
        }

        [CustomEditor(typeof (Actor))]
        // ReSharper disable once UnusedMember.Local
        private class ActorEditor : Editor {
            public override void OnInspectorGUI() {
                DrawDefaultInspector();

                var actor = (Actor) target;

                GUILayout.Label("Orders: ");

                if (actor.currentOrder != null)
                    GUILayout.Label(actor.currentOrder.GetType().Name ?? "");

                foreach (BaseOrder order in actor.orderQueue)
                    GUILayout.Label(order.GetType().Name);
            }
        }
    }
}
