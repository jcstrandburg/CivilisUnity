using UnityEngine;
using System.Collections;

/// <summary>
/// Handles toggling the selection halo for selectable units
/// </summary>
/// <todo>Finish transitioning to projector rather than sprite based selection halos</todo>
public class SelectHalo : MonoBehaviour {

	private SpriteRenderer renderer;
    private Projector projector;

    /// <summary>
    /// Property indicating whether the selection halo should be rendered
    /// </summary>
	public bool Highlighted {
		get {
            if (projector) {
                return projector.enabled;
            } else {
                return renderer.enabled;
            }
        }
		set {
            if (projector) {
                projector.enabled = value;
            } else {
                renderer.enabled = value;
            }            
        }
	}

    // Handles Awake event
	void Awake() {
		renderer = GetComponent<SpriteRenderer>();
        if (renderer) {
            renderer.enabled = false;
        }
        projector = GetComponent<Projector>();
        if (projector) {
            projector.enabled = false;
        }
	}

    // Handles OnSelect event
	void OnSelect() {
        Highlighted = true;
	}

    // Handles OnDeselect event
	void OnDeselect() {
        Highlighted = false;
	}
}
