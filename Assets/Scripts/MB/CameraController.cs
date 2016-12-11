using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
    //scroll speed dampener
    public float transformSpeed; 
    //zoom speed dampener
    public float zoomSpeed; 
    //rotation dampener
    public float rotateSpeed; 
    //minimum camera fov
    public float minFov; 
    //maximum camera fov
    public float maxFov; 
    //my camera object
    public Camera cam; 

    //screen edge/mouse based scrolling controls
    public float screenMargin;    
    public float minScrollRatio;
    public bool edgeScrolling = false;

    //min distance from the ground
    public float hover = 5.0f;
    //current zoom level, range [1, -2]
    public float zoomLevel = 0.0f;
    //current x axis rotation level
    public float xRotateLevel = 0.0f;
    //max height in strategic zoom
    public float strategicZoomMaxHeight = 325.0f;
    //interpolation speed to reduce camera jerkiness
    public float lerpSpeed = 0.03f;
    //x axis rotation limiter in strategic zoom. (0.0 is no limit, 1.0f eliminates all rotation at minimum zoom level)
    public float strategicRotation = 0.25f;

    //target position being lerped to
    private Vector3 targetPos;
    //target x rotation being lerped to
    private Vector3 targetRot;
    //helper variables to serialize camera settings
    private Quaternion cameraRotation;

    [Inject]
    public GameController GameController { get; set; }
    [Inject]
    public GroundController GroundController { get; set; }

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

    // Use this for initialization
    void FixedUpdate() {
        float tranSpeed = transformSpeed * (2.0f - zoomLevel);

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
        if (edgeScrolling && !GameController.boxActive && mousePos.x >= 0.0f && mousePos.y >= 0.0 && mousePos.x < Screen.width && mousePos.y < Screen.width) {
            float xScroll = 0.0f, yScroll = 0.0f;
            if (mousePos.x < screenMargin && mousePos.x >= 0.0f) {
                xScroll = tranSpeed * (-1.0f + Mathf.Min(mousePos.x / screenMargin, 1.0f - minScrollRatio));
            }
            else if (mousePos.x > Screen.width - screenMargin && mousePos.x < Screen.width) {
                xScroll = tranSpeed * (1.0f - Mathf.Min((Screen.width - mousePos.x) / screenMargin, 1.0f - minScrollRatio));
            }
            if (mousePos.y < screenMargin && mousePos.y >= 0.0f) {
                yScroll = tranSpeed * (-1.0f + Mathf.Min(mousePos.y / screenMargin, 1.0f - minScrollRatio));
            }
            else if (mousePos.y > Screen.height - screenMargin && mousePos.y < Screen.height) {
                yScroll = tranSpeed * (1.0f - Mathf.Min((Screen.height - mousePos.y) / screenMargin, 1.0f - minScrollRatio));
            }
            targetPos += transformSpeed * (xScroll * transform.right + yScroll * transform.forward);
        }

    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(2)) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (Input.GetMouseButtonUp(2)) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        //scroll to zoom
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel != 0.0f) {
            zoomLevel += scrollWheel * zoomSpeed;
            zoomLevel = Mathf.Clamp(zoomLevel, -2.0f, 1.0f);
        }

        //camera rotate when middle mouse depressed
        if (Input.GetMouseButton(2)) {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            //y rotation
            transform.eulerAngles = transform.eulerAngles + new Vector3(0.0f, rotateSpeed * mouseX, 0.0f);
            xRotateLevel = Mathf.Clamp(xRotateLevel - rotateSpeed * 0.02f * mouseY, 0.0f, 1.0f);
        }

        if (zoomLevel > 0.0f) {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, Mathf.Lerp(maxFov, minFov, zoomLevel), lerpSpeed);
            targetRot = new Vector3(Mathf.Lerp(7.0f, 85.0f, xRotateLevel),
                                    cam.transform.eulerAngles.y,
                                    cam.transform.eulerAngles.z);
            //hover off the ground, hover a little higher when pointing towards the ground
            float hoverBias = 1.0f + (cam.transform.eulerAngles.x / 90.0f);
            Vector3 intermediate = GameController.SnapToGround(targetPos);
            intermediate += new Vector3(0.0f, Mathf.Max(0.0f, GroundController.waterLevel - intermediate.y), 0.0f);
            targetPos = intermediate + new Vector3(0.0f, hoverBias * hover, 0.0f);
        } else {
            float z = -zoomLevel/2.0f;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, maxFov, lerpSpeed);
            float xRot = Mathf.Lerp(7.0f, 85.0f, xRotateLevel);
            targetRot = new Vector3(Mathf.Lerp(xRot, 90.0f, Mathf.Min(z * strategicRotation, 1.0f)),
                                    cam.transform.eulerAngles.y,
                                    cam.transform.eulerAngles.z);
            Vector3 tempPos = new Vector3(targetPos.x, Mathf.Lerp(hover * 2, strategicZoomMaxHeight, z), targetPos.z);
            Vector3 tempPos2 = GameController.SnapToGround(tempPos);
            targetPos = new Vector3(tempPos.x, Mathf.Max(tempPos.y, tempPos2.y + hover * 2), tempPos.z);
        }

        transform.position = Vector3.Lerp(transform.position, targetPos, lerpSpeed);
        cam.transform.eulerAngles = Vector3.Lerp(cam.transform.eulerAngles, targetRot, lerpSpeed);
    }
}
