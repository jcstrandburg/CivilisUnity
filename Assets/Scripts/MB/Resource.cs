using UnityEngine;
using System.Collections;

public class Resource : MonoBehaviour {
    public const float lifeTime = 60.0f;
	public float timer;
    public string typeTag;
    public double amount = 0;
    public bool preserved;

    //[DontSaveField]
    //private NeolithicObject neolithicObject;

    private GameController _gameController;
    [Inject]
    public GameController gameController {
        get {
            if (_gameController == null) {
                _gameController = GameController.Instance;
            }
            return _gameController;
        }
        set { _gameController = value; }
    }

    void Awake() {
        //neolithicObject = GetComponent<NeolithicObject>();
    }

	void Start() {
        if (timer == 0.0) {
            timer = lifeTime;
        }
        //not sure why i thought this was a good idea
        //preserved = true;
	}

	void FixedUpdate() {
        if (!preserved) {
            timer -= Time.fixedDeltaTime;
            if (timer <= 0) {
                //Debug.Log("Shoop da woop");
                Destroy(gameObject);
            }
        }

        //if (neolithicObject) {
        //    neolithicObject.statusString = string.Format("{0} {1} {2:0.0}", amount, typeTag, timer);
        //}
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

        gameObject.AddComponent<LogisticsNode>();
        Warehouse w = gameObject.AddComponent<Warehouse>();
        ResourceProfile[] rp = new ResourceProfile[] {
            new ResourceProfile(typeTag, amount)
        };

        w.SetLimits(rp);
        w.SetContents(rp);
        gameController.factory.InjectObject(gameObject);
    }

    public void OnResourceWithdrawn() {
        Warehouse w = GetComponent<Warehouse>();
        if (w.GetTotalAnyContents() <= 0) {
            Destroy(gameObject);
        }
    }
}
