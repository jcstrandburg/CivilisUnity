using UnityEngine;
using System.Collections;

public abstract class ActorOrder2d {
	public bool finished = false;

	public abstract void DoStep();

	public virtual void Being() {}

	public virtual void Cancel() {
		finished = true;
	}

	public virtual void Finish() {
		finished = true;
	}
}

public class IdleOrder2d : ActorOrder2d {
	Vector3 origin;
	Vector3 target;
	GameObject actor;
	
	static float moveSpeed = 0.1f;
	static float range = 10.0f;

	public IdleOrder2d(GameObject obj) {
		actor = obj;
		target = origin = obj.transform.position;
	}

	public override void DoStep() {
		Vector3 diff = target - actor.transform.position;
		float mag = diff.magnitude;
		
		if (mag <= moveSpeed) {
			target = origin + new Vector3(Random.Range (-range, range), Random.Range (-range, range), 0.0f);
		}
		else {
			actor.transform.position += moveSpeed/mag * diff;
		}
	}
}

public class SimpleMoveOrder2d : ActorOrder2d {
	
	public Vector3 target;
	public float moveSpeed = .9f;
	GameObject actor;
	
	public SimpleMoveOrder2d(GameObject obj, Vector3 targetPos) {
		target = targetPos;
		actor = obj;
	}
	
	public void Init(Vector3 targetPos) {
		target = targetPos;
	}
	
	// Update is called once per frame
	public override void DoStep () {

		Vector3 diff = target - actor.transform.position;
		float mag = diff.magnitude;
		
		if (mag <= moveSpeed) {
			Finish ();
		}
		else {
			actor.transform.position += moveSpeed/mag * diff;
		}
	}
}