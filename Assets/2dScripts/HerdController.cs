using UnityEngine;
using System.Collections;

public class HerdController : MonoBehaviour {

	public GameObject herdLure;
	public GameObject[] waypoints;
	public float migrateSpeed;
	public float moveSpeed;
	int targetWaypoint = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void FixedUpdate() {
		Vector3 diff = waypoints[targetWaypoint].transform.position - herdLure.transform.position;
		float magnitude = diff.magnitude;
		if ( diff.magnitude < moveSpeed) {
			targetWaypoint = (targetWaypoint+1)%waypoints.Length;
		}
		else {
			herdLure.transform.position += (moveSpeed/magnitude)*diff;
		}
	}
}
