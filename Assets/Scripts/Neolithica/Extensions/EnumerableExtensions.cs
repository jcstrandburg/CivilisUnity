using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Neolithica.Extensions {
    public static class EnumerableExtensions {
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source) {
            return source ?? new T[] {};
        }
    }
}