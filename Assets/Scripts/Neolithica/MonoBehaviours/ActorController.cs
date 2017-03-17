using System.Collections.Generic;
using Neolithica.MonoBehaviours.Logistics;
using Neolithica.MonoBehaviours.Reservations;
using Neolithica.Orders.Simple;
using Tofu.Serialization;
using UnityEngine;

namespace Neolithica.MonoBehaviours {
    [SavableMonobehaviour(17)]
    public class ActorController : NeolithicObject {
        public float moveSpeed;
        public List<BaseOrder> orderQueue = new List<BaseOrder>();
        public BaseOrder currentOrder = null;
        public bool idle = false;
        public bool followContours = true;
        public double health = 1.0;

        public int queueLength = 0;

        public StorageReservation storageReservation { get { return GetComponentInChildren<StorageReservation>(); } }
        public ResourceReservation resourceReservation { get { return GetComponentInChildren<ResourceReservation>(); } }
        public LogisticsActor logisticsActor;

        // Handles Start event
        public override void Start () {
            base.Start();
            logisticsActor = GetComponent<LogisticsActor>();
        }

        // Handles OnDestroy event
        public void OnDestroy() {
            if (currentOrder != null) {
                currentOrder.Cancel();
            }
            Reservation[] res = GetComponents<Reservation>();
            foreach (var r in res) {
                r.Cancelled = true;
            }
        }

        // Handles FixedUpdate event
        public virtual void FixedUpdate() {
            double feedMe = (double)(Time.fixedDeltaTime * 0.025f);
            var logisticsManager = logisticsActor.logisticsManager;
            LogisticsNetwork network = logisticsManager ? logisticsManager.FindNearestNetwork(transform.position) : null;

            if (network != null && network.foodbuffer > feedMe) {
                network.foodbuffer -= feedMe;
                health = System.Math.Min(1, health + feedMe);
            } else {
                health -= feedMe;
            }

            if (health <= 0.0) {
                Destroy(gameObject);
                return;
            }

            if (currentOrder != null && currentOrder.Done) {
                currentOrder = null;
            }

            if (currentOrder == null) {
                if (orderQueue.Count > 0) {
                    currentOrder = DequeueOrder();
                    idle = false;
                } else {
                    currentOrder = new IdleOrder(this);
                    idle = true;
                }
            }

            currentOrder.Update(this);
            queueLength = orderQueue.Count;
        }

        /// <summary>
        /// Gets and removes the first order from the order queue
        /// </summary>
        /// <returns>The first order from the queue</returns>
        public BaseOrder DequeueOrder() {
            //this is technically not safe, and should probably be changed back to a queue after i find a way to display the queue in the editor
            BaseOrder o = orderQueue[0];
            orderQueue.RemoveAt(0);
            return o;
        }

        /// <summary>Adds an order to the pending order queue</summary>
        public void EnqueueOrder(BaseOrder o) {
            orderQueue.Add(o);
            //orderQueue.Enqueue(o);
        }

        /// <summary>Clears the order queue and sets the current order</summary>
        public void OverrideOrder(BaseOrder o) {
            orderQueue.Clear();
            if (currentOrder != null) {
                currentOrder.Cancel();
            }
            idle = false;
            currentOrder = o;
        }

        /// <summary>Moves towards the given target position, returning true if the position has been reached at the end of the move</summary>
        public bool MoveTowards(Vector3 targetPosition, float moveRatio=1.0f) {
            Vector3 diff = targetPosition - transform.position;
            if (diff.magnitude <= moveSpeed*moveRatio) {
                transform.position = targetPosition;
                SnapToGround();
                return true;
            }
            else {
                if (followContours && Terrain.activeTerrain) {
                    Vector3 normal = Terrain.activeTerrain.terrainData.GetInterpolatedNormal(transform.position.x, transform.position.y);
                    Vector3 movDir = diff - Vector3.Project(diff, normal);

                    transform.position += movDir.normalized * moveSpeed * moveRatio;
                    SnapToGround();
                    return false;
                } else {
                    transform.position += diff * (moveSpeed / diff.magnitude) * moveRatio;
                    SnapToGround();
                    return false;
                }
            }
        }

        /// <summary>
        /// Finds and returned the first found child object that has the tag "Resource" and has a Resource component
        /// </summary>
        /// <param name="resourceKind"></param>
        /// <returns>A Resource object</returns>
        public Resource GetCarriedResource(ResourceKind? resourceKind=null) {
            foreach (Transform t in transform) {
                if (t.gameObject == null || t.tag != "Resource") continue;
                Resource r = t.GetComponent<Resource>();
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
            float range = 3.5f;
            Vector3 randomVector = new Vector3(UnityEngine.Random.Range(-range, range), .1f, UnityEngine.Random.Range(-range, range));
            r.transform.position = transform.position + randomVector;
            r.SetDown();
        }

        /// <summary>
        /// Picks up the given resource. It is assumed that the give
        /// </summary>
        /// <param name="res"></param>
        public void PickupResource(Resource res) {
            res.transform.SetParent(this.transform);
            res.transform.localPosition = new Vector3(0, 4.0f, 0);
            res.Pickup();
        }

        /// <summary>
        /// Starts the actor panicing, pauses current orders and starts executing the given response order
        /// </summary>
        /// <param name="response">Response order</param>
        public void Panic(BaseOrder response) {
            if (!idle) {
                Debug.Log("Pausing order!");
                currentOrder.Pause();
                orderQueue.Insert(0, currentOrder);
            }
            currentOrder = response;
        }
    }
}
