using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActorController : NeolithicObject {
	public float moveSpeed;
	public Queue<BaseOrder> orderQueue = new Queue<BaseOrder>();
	public BaseOrder currentOrder = null;
	public bool idle = false;
    public bool followContours = true;

    public StorageReservation storageReservation { get { return GetComponentInChildren<StorageReservation>(); } }
    public ResourceReservation resourceReservation { get { return GetComponentInChildren<ResourceReservation>(); } }

	// Use this for initialization
	public override void Start () {
		base.Start();
	}

	public virtual void FixedUpdate() {
		if (currentOrder != null && currentOrder.completed) {
			//Debug.Log("Removing completed order");
			currentOrder = null;
			//Debug.Log(currentOrder);
		}

		if (currentOrder == null) {
			if (orderQueue.Count > 0) {
				//Debug.Log("Retrieving queued order");
				currentOrder = DequeueOrder();
			} else {
				currentOrder = new IdleOrder(this);
				idle = true;
			}
		}

		if (currentOrder != null) {
			//Debug.Log("Doing step on current");
			currentOrder.DoStep();
		}
	}

	public BaseOrder DequeueOrder() {
		return orderQueue.Dequeue();
	}

	/// <summary>Adds an order to the pending order queue</summary>
	public void EnqueueOrder(BaseOrder o) {
		orderQueue.Enqueue(o);
	}

	/// <summary>Clears the order queue and sets the current order</summary>
	public void OverrideOrder(BaseOrder o) {
		orderQueue.Clear();
		if (currentOrder != null) {
			currentOrder.Cancel();
		}
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
            if (followContours) {
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

    public Resource GetCarriedResource(string resourceType=null) {
        //Debug.Log("Getting carried resource");
        foreach (Transform t in transform) {
            if (t.gameObject != null && t.tag == "Resource") {
                Resource r = t.GetComponent<Resource>();
                if (resourceType == null || r.typeTag == resourceType) {
                    return r;
                }
            }
        }
        return null;
    }

    public void CarryResource(Resource r) {
        if (!r) {
            Debug.Log(r);
            throw new System.Exception("Cannot carry resource!");
        }
    }

    public void DropCarriedResource() {
        Resource r = GetCarriedResource();
        if (!r) {
            throw new System.Exception("No carried resource to drop");
        }
        r.SetDown();
        r.transform.SetParent(transform.parent);
        float range = 4.0f;
        Vector3 randomVector = new Vector3(UnityEngine.Random.Range(-range, range), .1f, UnityEngine.Random.Range(-range, range));
        r.transform.position = transform.position + randomVector;
    }

    public void PickupResource(GameObject res) {
        res.transform.SetParent(this.transform);
        res.transform.localPosition = new Vector3(0, 4.0f, 0);
        res.GetComponent<Resource>().Pickup();
    }
}
