using UnityEngine;
using System.Collections;

public class Resource : MonoBehaviour {
    public const float lifeTime = 12.0f;
	public float timer;
    public string typeTag;
    public float amount = 0.0f;
    public bool preserved;

    private NeolithicObject neolithicObject;

	void Start() {
        timer = lifeTime;
        preserved = true;
        neolithicObject = GetComponent<NeolithicObject>();
	}

	void FixedUpdate() {
        if (!preserved) {
            timer -= Time.fixedDeltaTime;
            if (timer < 0) {
                Destroy(gameObject);
            }
        }

        neolithicObject.statusString = string.Format("{0} {1} {2:0.0}", amount, typeTag, timer);
	}
}
