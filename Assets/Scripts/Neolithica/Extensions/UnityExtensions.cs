using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Neolithica.Extensions {
    public static class UnityExtensions {
        public static IEnumerable<GameObject> Children(this GameObject source) {
            return from Transform t in source.transform select t.gameObject;
        }

        public static IEnumerable<GameObject> Children(this Component source) {
            return from Transform t in source.transform select t.gameObject;
        }

        /// <summary>
        /// Helper for caching component references.
        /// 
        /// Replace:
        /// if (cachedComponent == null) {
        ///     cachedComponent = FindObjectOfType<ComponentType>();
        /// }
        /// return cachedComponent;
        /// 
        /// With:
        /// return this.CacheComponent(ref cachedComponent, FindObjectOfType<ComponentType>);
        /// </summary>
        public static T CacheComponent<T>(this Component source, ref T data, Func<T> locator)
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression (Unity fake null)
            if (data == null)
                data = locator();

            return data;
        }

        /// <summary>
        /// Helper for caching component references.
        /// 
        /// Replace:
        /// if (cachedComponent == null) {
        ///     cachedComponent = FindObjectOfType<ComponentType>();
        /// }
        /// return cachedComponent;
        /// 
        /// With:
        /// return UnityExtensions.CacheComponent(ref cachedComponent, FindObjectOfType<ComponentType>);
        /// </summary>
        public static T CacheComponent<T>(ref T data, Func<T> locator)
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression (Unity fake null)
            if (data == null)
                data = locator();

            return data;
        }

        /// <summary>
        /// Helper extension method to replace the elvis operator. This is necessary because Unity has a "fake null"
        /// for destroyed Components that is not caught by ?. and ?? operators.
        /// </summary>
        public static TReturn Elvis<TReturn, TComponent>(this TComponent source, Func<TComponent, TReturn> fnGet)
            where TReturn: class
            where TComponent: Component  =>
            source == null ? null : fnGet(source);

        public static Vector3 Copy(this Vector3 src, float? x = null, float? y = null, float? z = null) =>
            new Vector3(x ?? src.x, y ?? src.y, z ?? src.z);
    }
}