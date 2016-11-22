using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[Serializable]
public class EventListen {
    string eventType;
    UnityEngine.Events.UnityAction action;
}

public class EventListener : MonoBehaviour {

    public List<EventListen> listeners;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
