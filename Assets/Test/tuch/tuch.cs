using UnityEngine;
using System.Collections;

public class tuch : MonoBehaviour {

	public GameObject cam;
	public GameObject boy;
	public GameObject girl;
	public GameObject cat;
	public GameObject hut;
	public GameObject tree;
	public GameObject rock;
	public float orthoZoom = 0.5f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Touch[] touches = Input.touches;
		//Debug.Log(touches.Length);

		if (touches.Length == 1) {

			Vector3 touchPos = Camera.main.ScreenToWorldPoint(touches[0].position) + new Vector3(0,0,10.0f);
			switch (touches[0].phase) { 
			case TouchPhase.Began:
				boy.transform.position = touchPos;
				break;
			case TouchPhase.Canceled:
				cat.transform.position = touchPos;
				break;
			case TouchPhase.Moved:
				tree.transform.position = touchPos;
				break;
			case TouchPhase.Stationary:
				rock.transform.position = touchPos;
				break;
			case TouchPhase.Ended:
				hut.transform.position = touchPos;
				break;
			}
			if (touches[0].phase == TouchPhase.Ended) {

				switch (touches[0].tapCount) {
//				case 1:
//					boy.transform.position += new Vector3(10.0f, 0);
//					boy.transform.position = touchPos + new Vector3(0, 0, 10.0f);
//					break;
				case 2:
					girl.transform.position += new Vector3(10.0f, 0);
					girl.transform.position = touchPos + new Vector3(0, 0, 10.0f);
					break;
				}
			}

			if (touches[0].phase == TouchPhase.Moved) {
				Vector2 mv = touches[0].deltaPosition;
				cam.transform.position += new Vector3(mv.x, mv.y, 0);
			}

		} else if (touches.Length == 2) {
			// Store both touches.
			Touch touchZero = Input.GetTouch(0);
			Touch touchOne = Input.GetTouch(1);
			
			// Find the position in the previous frame of each touch.
			Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
			Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
			
			// Find the magnitude of the vector (the distance) between the touches in each frame.
			float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
			
			// Find the difference in the distances between each frame.
			float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;


			Camera camera = cam.GetComponent<Camera>();
			camera.orthographicSize += deltaMagnitudeDiff * orthoZoom;			
			// Make sure the orthographic size never drops below zero.
			camera.orthographicSize = Mathf.Max(camera.orthographicSize, 0.1f);
		}

	}
}
