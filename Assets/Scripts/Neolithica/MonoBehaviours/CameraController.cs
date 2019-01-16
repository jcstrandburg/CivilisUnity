using System;
using System.Diagnostics.CodeAnalysis;
using Tofu.Serialization;
using UnityEngine;

#pragma warning disable 649
namespace Neolithica.MonoBehaviours {
    [SavableMonobehaviour(15)]
    public class CameraController : MonoBehaviour {
        [SerializeField] private CameraControllerSettings settings;

        //current zoom level, range [1, -2]
        [SerializeField] private float zoomLevel = 0.0f;
        //current x axis rotation level
        [SerializeField] private float xRotateLevel = 0.0f;
        [SerializeField] private Camera cam; 

        //target position being lerped to
        [SerializeField] private Vector3 targetPos;
        //target x rotation being lerped to
        [SerializeField] private Vector3 targetRot;
        //helper variables to serialize camera settings
        [SerializeField] private Quaternion cameraRotation;

        [Inject] public GameController GameController { get; set; }
        [Inject] public GroundController GroundController { get; set; }

        public void Start() {
            targetPos = transform.position;
            targetRot = cam.transform.eulerAngles;
        }

        public void OnSerialize() {
            cameraRotation = cam.transform.rotation;
        }

        public void OnDeserialize() {
            cam.transform.rotation = cameraRotation;
        }

        // ReSharper disable once UnusedMember.Local (Unity method)
        private void FixedUpdate() {
            float tranSpeed = settings.TransformSpeed * (2.0f - zoomLevel);

            //keyboard scrolling
            if (Input.GetKey("a")) {
                targetPos -= tranSpeed * transform.right;
            }
            else if (Input.GetKey("d")) {
                targetPos += tranSpeed * transform.right;
            }
            if (Input.GetKey("w")) {
                targetPos += tranSpeed * transform.forward;
            }
            else if (Input.GetKey("s")) {
                targetPos -= tranSpeed * transform.forward;
            }

            //screen edge scrolling
            Vector3 mousePos = Input.mousePosition;
            if (settings.EdgeScrolling && !GameController.boxActive && mousePos.x >= 0.0f && mousePos.y >= 0.0 && mousePos.x < Screen.width && mousePos.y < Screen.width) {
                float xScroll = 0.0f, yScroll = 0.0f;
                if (mousePos.x < settings.ScreenMargin && mousePos.x >= 0.0f) {
                    xScroll = tranSpeed * (-1.0f + Mathf.Min(mousePos.x / settings.ScreenMargin, 1.0f - settings.MinScrollRatio));
                }
                else if (mousePos.x > Screen.width - settings.ScreenMargin && mousePos.x < Screen.width) {
                    xScroll = tranSpeed * (1.0f - Mathf.Min((Screen.width - mousePos.x) / settings.ScreenMargin, 1.0f - settings.MinScrollRatio));
                }
                if (mousePos.y < settings.ScreenMargin && mousePos.y >= 0.0f) {
                    yScroll = tranSpeed * (-1.0f + Mathf.Min(mousePos.y / settings.ScreenMargin, 1.0f - settings.MinScrollRatio));
                }
                else if (mousePos.y > Screen.height - settings.ScreenMargin && mousePos.y < Screen.height) {
                    yScroll = tranSpeed * (1.0f - Mathf.Min((Screen.height - mousePos.y) / settings.ScreenMargin, 1.0f - settings.MinScrollRatio));
                }
                targetPos += settings.TransformSpeed * (xScroll * transform.right + yScroll * transform.forward);
            }

        }

        // ReSharper disable once UnusedMember.Local (Unity method)
        private void Update() {
            if (GameController.Paused) {
                return;
            }

            if (Input.GetMouseButtonDown(2)) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (Input.GetMouseButtonUp(2)) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            // scroll to zoom
            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
            zoomLevel += scrollWheel * settings.ZoomSpeed;
            zoomLevel = Mathf.Clamp(zoomLevel, -2.0f, 1.0f);

            // camera rotate when middle mouse depressed
            if (Input.GetMouseButton(2)) {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                // y rotation
                transform.eulerAngles = transform.eulerAngles + new Vector3(0.0f, settings.RotateSpeed * mouseX, 0.0f);
                xRotateLevel = Mathf.Clamp(xRotateLevel - settings.RotateSpeed * 0.02f * mouseY, 0.0f, 1.0f);
            }

            if (zoomLevel > 0.0f) {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, Mathf.Lerp(settings.MaxFov, settings.MinFov, zoomLevel), settings.LerpSpeed);
                targetRot = new Vector3(Mathf.Lerp(7.0f, 85.0f, xRotateLevel),
                    cam.transform.eulerAngles.y,
                    cam.transform.eulerAngles.z);

                // hover off the ground, hover a little higher when pointing towards the ground
                float hoverBias = 1.0f + (cam.transform.eulerAngles.x / 90.0f);
                Vector3 intermediate = GameController.SnapToGround(targetPos);
                intermediate += new Vector3(0.0f, Mathf.Max(0.0f, GroundController.waterLevel - intermediate.y), 0.0f);
                targetPos = intermediate + new Vector3(0.0f, hoverBias * settings.MinHoverDistance, 0.0f);
            } else {
                float z = -zoomLevel / 2.0f;
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, settings.MaxFov, settings.LerpSpeed);
                float xRot = Mathf.Lerp(7.0f, 85.0f, xRotateLevel);
                targetRot = new Vector3(Mathf.Lerp(xRot, 90.0f, Mathf.Min(z * settings.StrategicRotation, 1.0f)),
                    cam.transform.eulerAngles.y,
                    cam.transform.eulerAngles.z);
                Vector3 tempPos = new Vector3(targetPos.x, Mathf.Lerp(settings.MinHoverDistance * 2, settings.MaxStrategicZoomHeight, z), targetPos.z);
                Vector3 tempPos2 = GameController.SnapToGround(tempPos);
                targetPos = new Vector3(tempPos.x, Mathf.Max(tempPos.y, tempPos2.y + settings.MinHoverDistance * 2), tempPos.z);
            }

            transform.position = Vector3.Lerp(transform.position, targetPos, settings.LerpSpeed);
            cam.transform.eulerAngles = Vector3.Lerp(cam.transform.eulerAngles, targetRot, settings.LerpSpeed);
        }

        [Serializable]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        [SuppressMessage("ReSharper", "ConvertToConstant.Local")]
        private class CameraControllerSettings {
            public float TransformSpeed = 2f;
            public float ZoomSpeed = 0.5f;
            public float RotateSpeed = 2f;
            public float MinFov = 25f;
            public float MaxFov = 80f;
            public float MinHoverDistance = 5f;
            public float MaxStrategicZoomHeight = 500f;
            public float LerpSpeed = 0.1f;

            //screen edge/mouse based scrolling controls
            public float ScreenMargin = 50f;
            public float MinScrollRatio = 0.2f;
            public bool EdgeScrolling = false;

            //x axis rotation limiter in strategic zoom. (0.0 is no limit, 1.0f eliminates all rotation at minimum zoom level)
            public float StrategicRotation = 0.7f;
        }
    }
}
