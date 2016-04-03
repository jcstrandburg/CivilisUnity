using UnityEngine;
using System.Collections;

public class HerdFollowerController : MonoBehaviour {

	public GameObject herdLure;
	public float moveSpeed = 0.35f;

	Vector3 targetLocation;

	void FindTarget() {
		targetLocation = herdLure.transform.position;
		targetLocation += new Vector3(Random.Range (-10.0f,10.0f),
		                              Random.Range (-10.0f,10.0f),
		                              0.0f);
	}

	// Use this for initialization
	void Start () {
		FindTarget ();
	}

	void FixedUpdate() {
		Vector3 diff = targetLocation - transform.position;
		float magnitude = diff.magnitude;

		if ( magnitude < moveSpeed) {
			FindTarget ();
		}
		else {
			transform.position += (moveSpeed/magnitude)*diff;
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
