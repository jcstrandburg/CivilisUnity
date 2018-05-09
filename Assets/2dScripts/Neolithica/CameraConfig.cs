using UnityEngine;

namespace Neolithica {
    public class CameraConfig : ScriptableObject {
        public float MoveSpeed = 0.1f;
        public float ZoomExponent = 1.0f;
        public float ZoomSpeed = 0.05f;
        public float MinZ = -180.0f;
        public float MaxZ = -40.0f;
        public float MinOrthoSize = 20.0f;
        public float MaxOrthoSize = 100.0f;
    }
}
