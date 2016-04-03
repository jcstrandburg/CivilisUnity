using UnityEngine;
using System.Collections;

public class Reservation : MonoBehaviour {

	private bool ready = false;
	private bool acknowledged = false;
	private bool released = false;

	public bool Ready {
		get { return ready; }
		set { ready = value; }
	}

	public bool Acknowledged {
		get { return acknowledged; }
		set { acknowledged = value; }
	}

	public bool Released {
		get { return released; }
		set { 
			released = value; 
			if (released) {

			}
		}
	}
}

