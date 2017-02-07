using UnityEngine;

namespace Neolithica.MonoBehaviours {
    public class BuildingBlueprint : MonoBehaviour {

        ConstructionManager constructMe;

        GameObject prefab;

        // Use this for initialization
        void Start () {
            constructMe = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefab"></param>
        /// <todo>This needs to get updated to not spawn an actual building until placement or something</todo>
        public void Activate(GameObject prefab) {
            //this is probably not the best way to handle this, deal with this later
            GameObject go = GameController.Instance.Factory.Instantiate(prefab);

            constructMe = go.GetComponent<ConstructionManager>();
            constructMe.transform.position = transform.position;
            constructMe.StartPlacement();
            go.transform.SetParent(transform);
        }

        public void Deactivate() {
            constructMe = null;
        }


        // Update is called once per frame
        void Update() {
            if (constructMe == null) {
                return;
            }

            if (Input.GetMouseButtonDown(1)) {
                Destroy(constructMe.gameObject);
                constructMe = null;
                Deactivate();
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, ray.direction, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Terrain"))) {
                transform.position = hit.point;
                bool elligible = constructMe.IsBuildable(transform.position);
                if (elligible) {
                    constructMe.GhostGood();
                    if (Input.GetMouseButtonDown(0)) {
                        constructMe.transform.SetParent(null);
                        constructMe.StartConstruction();
                        Deactivate();
                    }
                } else {
                    constructMe.GhostBad();
                }
            }
            else {

            }
        }
    }
}
