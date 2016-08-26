using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ActorController))]
public class ActorControllerEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        ActorController a = (ActorController)target;
        if (GUILayout.Button("Do Stuff")) {
            a.palzy.transform.position += new Vector3(1.0f, 0);
        }
        if (GUILayout.Button("Panic")) {
            GameObject go = GameObject.Find("DumpingGround");
            var order = new SimpleMoveOrder(a, go.transform.position);
            a.Panic(order);
        }
    }
}
#endif


public class ActorController : NeolithicObject {
	public float moveSpeed;
    public List<BaseOrder> orderQueue = new List<BaseOrder>();
	public BaseOrder currentOrder = null;
	public bool idle = false;
    public bool followContours = true;
    public double health = 1.0;

    public GameObject palzy;//temp debugging object
    public int queueLength = 0;

    public StorageReservation storageReservation { get { return GetComponentInChildren<StorageReservation>(); } }
    public ResourceReservation resourceReservation { get { return GetComponentInChildren<ResourceReservation>(); } }
    public LogisticsActor logisticsActor;

	// Use this for initialization
	public override void Start () {
		base.Start();
        logisticsActor = GetComponent<LogisticsActor>();
	}

    public void OnDestroy() {
        if (currentOrder != null) {
            currentOrder.Cancel();
        }
        Reservation[] res = GetComponents<Reservation>();
        foreach (var r in res) {
            r.Cancelled = true;
        }
    }

	public virtual void FixedUpdate() {
        double feedMe = (double)(Time.fixedDeltaTime * 0.025f);
        LogisticsNetwork network = logisticsActor.logisticsManager.FindNearestNetwork(transform.position);

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

		currentOrder.Update();
        queueLength = orderQueue.Count;
	}

	public BaseOrder DequeueOrder() {
        //this is technically not safe, and should probably be changed back to a queue after i find a way to display the queue in the editor
        BaseOrder o = orderQueue[0];
        orderQueue.RemoveAt(0);
        return o;
        //return orderQueue.Dequeue();
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
            Debug.Log("No carried resource to drop");
            return;
        }
        float range = 3.5f;
        Vector3 randomVector = new Vector3(UnityEngine.Random.Range(-range, range), .1f, UnityEngine.Random.Range(-range, range));
        r.transform.position = transform.position + randomVector;
        r.SetDown();
    }

    public void PickupResource(GameObject res) {
        res.transform.SetParent(this.transform);
        res.transform.localPosition = new Vector3(0, 4.0f, 0);
        res.GetComponent<Resource>().Pickup();
    }

    public void Panic(BaseOrder response) {
        if (!idle) {
            Debug.Log("Pausing order!");
            currentOrder.Pause();
            orderQueue.Insert(0, currentOrder);
        }
        currentOrder = response;
    }
}
