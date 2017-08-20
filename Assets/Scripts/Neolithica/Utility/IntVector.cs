using UnityEngine;

namespace Neolithica.Utility {
    public struct IntVector {
        public int x;
        public int y;

        public IntVector(int pX, int pY) {
            x = pX;
            y = pY;
        }

        public override string ToString() {
            return string.Format("({0}, {1})", x, y);
        }

        public static explicit operator IntVector(Vector2 source) {
            return new IntVector {
                x = (int)source.x,
                y = (int)source.y,
            };
        }
    }
}