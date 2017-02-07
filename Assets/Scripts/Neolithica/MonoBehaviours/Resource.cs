using Neolithica.MonoBehaviours.Logistics;
using UnityEngine;

namespace Neolithica.MonoBehaviours {
    public class Resource : MonoBehaviour {
        public enum Type {
            Meat,
            Wood,
            Gold,
            Fish,
            Vegetables,
            Stone,
        }

        public const float lifeTime = 60.0f;
        public float timer;
        public Type type;
        public double amount = 0;
        public bool preserved;

        [Inject]
        public GameController GameController { get; set; }

        public void Awake() {
        }

        public void Start() {
            if (timer == 0.0) {
                timer = lifeTime;
            }
            //not sure why i thought this was a good idea
            //preserved = true;
        }

        public void FixedUpdate() {
            if (!preserved) {
                timer -= Time.fixedDeltaTime;
                if (timer <= 0) {
                    //Debug.Log("Shoop da woop");
                    Destroy(gameObject);
                }
            }
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
            var neoObject = GetComponent<NeolithicObject>();
            neoObject.snapToGround = true;
            neoObject.selectable = true;
            neoObject.SnapToGround();

            transform.parent = transform.parent ? transform.parent.parent : null;
            GameController.Factory.AddComponent<LogisticsNode>(gameObject);
            Warehouse w = GameController.Factory.AddComponent<Warehouse>(gameObject);
            ResourceProfile[] rp = new ResourceProfile[] {
                new ResourceProfile(type, amount)
            };

            w.SetLimits(rp);
            w.SetContents(rp);
        }

        public void OnResourceWithdrawn() {
            Warehouse w = GetComponent<Warehouse>();
            if (w.GetTotalAnyContents() <= 0) {
                Destroy(gameObject);
            }
        }
    }
}
