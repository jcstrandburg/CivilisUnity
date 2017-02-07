using System.Collections.Generic;
using UnityEngine;

namespace Neolithica.Extensions {
    public static class UnityExtensions {
        public static IEnumerable<GameObject> Children(this GameObject source) {
            foreach (Transform t in source.transform) {
                yield return t.gameObject;
            }
        }

        public static IEnumerable<GameObject> Children(this Component source) {
            foreach (Transform t in source.transform) {
                yield return t.gameObject;
            }
        }
    }
}