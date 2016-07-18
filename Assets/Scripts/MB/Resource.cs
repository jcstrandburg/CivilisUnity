using UnityEngine;
using System.Collections;

public class Resource : MonoBehaviour {
    public const float lifeTime = 60.0f;
	public float timer;
    public string typeTag;
    public float amount = 0.0f;
    public bool preserved;

    [DontSaveField]
    private NeolithicObject neolithicObject;

    void Awake() {
        neolithicObject = GetComponent<NeolithicObject>();
    }

	void Start() {
        timer = lifeTime;
        preserved = true;
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

    public void Pickup() {
        NeolithicObject neoObject = GetComponent<NeolithicObject>();
        neoObject.snapToGround = false;
        preserved = true;
        GetComponent<NeolithicObject>().selectable = false;
        Warehouse w = GetComponent<Warehouse>();
        if (w) {
            Destroy(w);
        }
    }

    public void SetDown() {
        preserved = false;
        NeolithicObject neoObject = GetComponent<NeolithicObject>();
        neoObject.snapToGround = true;
        neoObject.selectable = true;
        neoObject.SnapToGround();
        //GetComponent<NeolithicObject>().selectable = true;
        Warehouse w = gameObject.AddComponent<Warehouse>();
        ResourceProfile[] rp = new ResourceProfile[] {
            new ResourceProfile(typeTag, amount)
        };
        w.SetLimits(rp);
        w.SetContents(rp);
    }

    public void OnResourceWithdrawn() {
        Warehouse w = GetComponent<Warehouse>();
        if (w.GetTotalAnyContents() <= 0.01f) {
            Destroy(gameObject);
        }
    }
}
