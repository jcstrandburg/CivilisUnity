using UnityEngine;
using System.Collections;

public class DebugMenu : MonoBehaviour {

    public bool test = false;

	// Use this for initialization
	void Start () {	
	}
	
	// Update is called once per frame
	void Update () {	
	}

    public void QuickSave() {
        GameController.instance.QuickSave();
    }

    public void QuickLoad() {
        GameController.instance.QuickLoad();
    }

    public void AddWorker() {
        Transform spawn = GameObject.Find("DumpingGround").transform;
        GameObject prefab = Resources.Load("Units/Worker") as GameObject;
        GameObject newWorker = Instantiate(prefab);
        newWorker.transform.position = spawn.position;
    }

    public void GameSpeed() {
        Time.timeScale *= 2;
        if (Time.timeScale > 4.0f) {
            Time.timeScale = 0.25f;
        }
        Debug.Log("Timescale: " + Time.timeScale);
    }
}
