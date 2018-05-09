using UnityEngine;

namespace Neolithica {
    public class SelectionHalo2D : MonoBehaviour {

        private SpriteRenderer spriteRenderer;

        public bool Highlighted {
            set { spriteRenderer.enabled = value; }
            get { return spriteRenderer.enabled; }
        }

        public void Awake() {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
}
