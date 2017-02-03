using UnityEngine;

/// <summary>
/// Handles toggling the selection halo for selectable units
/// </summary>
/// <todo>Finish transitioning to projector rather than sprite based selection halos</todo>
public class SelectHalo : MonoBehaviour {

	private SpriteRenderer spriteRenderer;
    private Projector projector;

    /// <summary>
    /// Property indicating whether the selection halo should be rendered
    /// </summary>
	public bool Highlighted {
		get {
            if (projector) {
                return projector.enabled;
            } else {
                return spriteRenderer.enabled;
            }
        }
		set {
            if (projector) {
                projector.enabled = value;
            } else {
                spriteRenderer.enabled = value;
            }            
        }
	}

    // Handles Awake event
	void Awake() {
		spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer) {
            spriteRenderer.enabled = false;
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
