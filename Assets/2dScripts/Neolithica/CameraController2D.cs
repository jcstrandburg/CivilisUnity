using Neolithica.Extensions;
using UnityEngine;
#pragma warning disable 649 // Unity initialized fields

namespace Neolithica {
    public class CameraController2D : MonoBehaviour {

        [SerializeField] private CameraConfig config;
        [SerializeField] private float zoomLevel = 1.0f;

        private Camera Camera => this.CacheComponent(ref camera, GetComponent<Camera>);
        private new Camera camera;

        //[Inject] private GameController GameController { get; set; }

        public void Start () {
        }

        public void FixedUpdate() {
            float tranSpeed = config.MoveSpeed / (zoomLevel + 0.05f);

            var move = new Vector3();

            //keyboard scrolling
            if (Input.GetKey("a"))
                move.x = -tranSpeed;
            else if (Input.GetKey("d"))
                move.x = tranSpeed;
            if (Input.GetKey("w"))
                move.y = tranSpeed;
            else if (Input.GetKey("s"))
                move.y = -tranSpeed;

            if (move != default(Vector3))
                transform.position = transform.position + move;
        }

        public void Update () {
            // scroll to zoom
            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
            zoomLevel += scrollWheel * config.ZoomSpeed;
            zoomLevel = Mathf.Clamp(zoomLevel, 0.0f, 1.0f);

            Camera.orthographicSize = Mathf.Lerp(config.MaxOrthoSize, config.MinOrthoSize, zoomLevel);
        }
    }
}
